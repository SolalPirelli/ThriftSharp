// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

[<AutoOpen>]
module ThriftSharp.Tests.Utils 
open System
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open System.Threading
open System.Threading.Tasks
open Xunit
open ThriftSharp
open ThriftSharp.Internals

/// Use Nullable without having to open System (because of conflicts with e.g. Int32 in memory protocol)
let nullable = System.Nullable
let date(dd, mm, yyyy) = DateTime(yyyy, mm, dd, 00, 00, 00, DateTimeKind.Utc)

/// Nicer syntax for assert equals, using deep equality
let (<=>) (act: 'a) (exp: 'a) =
    DeepEqual.Syntax.ObjectExtensions.ShouldDeepEqual(act, exp)


/// Converts a non-generic Task to a Task<unit>
let asUnit (t: Task) =
    let tcs = TaskCompletionSource<unit>()
    t.ContinueWith(fun _ -> match t.Status with
                            | TaskStatus.RanToCompletion -> tcs.SetResult(())
                            | TaskStatus.Faulted -> tcs.SetException(t.Exception.InnerException)
                            | _ -> tcs.SetCanceled()) |> ignore
    tcs.Task


/// Converts any Async to a non-generic Task
let asTask (a: Async<_>) =
    (Async.StartAsTask a) :> Task
    

/// Helper type because CustomAttributeBuilder is annoying
[<NoComparison>]
type AttributeInfo = 
    { typ: Type
      args: obj list
      namedArgs: (string * obj) list }
    static member AsBuilder (ai: AttributeInfo) =
        CustomAttributeBuilder(ai.typ.GetConstructors().[0], 
                               ai.args |> List.toArray, 
                               ai.namedArgs |> List.map (fst >> ai.typ.GetProperty) |> List.toArray, 
                               ai.namedArgs |> List.map snd |> List.toArray)

/// Creates an interface with the specified attributes and methods (parameters and their attributes, return type, method attributes)
let makeInterface (attrs: AttributeInfo list) 
                  (meths: ((Type * AttributeInfo list) list * Type * AttributeInfo list) list) =    
    let guid = Guid.NewGuid()
    let assemblyName = AssemblyName(guid.ToString())
    let moduleBuilder = Thread.GetDomain()
                              .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                              .DefineDynamicModule(assemblyName.Name)
    
    let interfaceBuilder = moduleBuilder.DefineType("GeneratedType", TypeAttributes.Interface ||| TypeAttributes.Abstract ||| TypeAttributes.Public)
    
    attrs |> List.iter (AttributeInfo.AsBuilder >> interfaceBuilder.SetCustomAttribute)

    meths |> List.iteri (fun n (args, retType, methAttrs) ->
        let methodBuilder = interfaceBuilder.DefineMethod(string n, 
                                                          MethodAttributes.Public ||| MethodAttributes.Abstract ||| MethodAttributes.Virtual, 
                                                          retType, 
                                                          args |> List.map fst |> List.toArray)

        args |> List.iteri (fun i (_, argAttrs) ->
             // i + 1 because 0 is the return value
            let paramBuilder = methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, string i)
            argAttrs |> List.iter (AttributeInfo.AsBuilder >> paramBuilder.SetCustomAttribute) )

        methAttrs |> List.iter (AttributeInfo.AsBuilder >> methodBuilder.SetCustomAttribute) )
    
    interfaceBuilder.CreateType().GetTypeInfo()

/// Creates a class with the specified attributes and properties (with their own attributes)
let makeClass (attrs: AttributeInfo list) 
              (props: (Type * AttributeInfo list) list) =
    let guid = Guid.NewGuid()
    let assemblyName = AssemblyName(guid.ToString())
    let moduleBuilder = Thread.GetDomain()
                              .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                              .DefineDynamicModule(assemblyName.Name)
    
    let typeBuilder = moduleBuilder.DefineType("GeneratedType", TypeAttributes.Class ||| TypeAttributes.Public)
    
    attrs |> List.iter (AttributeInfo.AsBuilder >> typeBuilder.SetCustomAttribute)

    props |> List.iteri (fun n (typ, attrs) ->
        let fieldBuilder = typeBuilder.DefineField("_" + n.ToString(), typ, FieldAttributes.Private)

        let getterBuilder = typeBuilder.DefineMethod("get_" + n.ToString(), MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig, typ, Type.EmptyTypes)
        let getterIL = getterBuilder.GetILGenerator()
        getterIL.Emit(OpCodes.Ldarg_0)
        getterIL.Emit(OpCodes.Ldfld, fieldBuilder)
        getterIL.Emit(OpCodes.Ret)
        
        let setterBuilder = typeBuilder.DefineMethod("set_" + n.ToString(), MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig, null, [| typ |])
        let setterIL = setterBuilder.GetILGenerator()
        setterIL.Emit(OpCodes.Ldarg_0)
        setterIL.Emit(OpCodes.Ldarg_1)
        setterIL.Emit(OpCodes.Stfld, fieldBuilder)
        setterIL.Emit(OpCodes.Ret)

        let propBuilder = typeBuilder.DefineProperty(n.ToString(), PropertyAttributes.None, typ, null)
        propBuilder.SetGetMethod(getterBuilder)
        propBuilder.SetSetMethod(setterBuilder)

        attrs |> List.iter (AttributeInfo.AsBuilder >> propBuilder.SetCustomAttribute) )

    typeBuilder.CreateType().GetTypeInfo()


let tid (n: int) = byte n |> LanguagePrimitives.EnumOfValue

let dict (vals: ('a * 'b) seq) =
    let dic = Dictionary()
    for (k,v) in vals do
        dic.Add(k, v)
    dic

let throws<'T when 'T :> exn> func =
    let exn = ref Unchecked.defaultof<'T>

    try
        func() |> ignore
    with
    | ex when typeof<'T>.IsAssignableFrom(ex.GetType()) -> 
        exn := ex :?> 'T
    | ex -> 
        Assert.True(false, sprintf "Expected an exception of type %A, but got one of type %A (message: %s)" typeof<'T> (ex.GetType()) ex.Message)
    
    if Object.Equals(!exn, null) then
        Assert.True(false, "Expected an exception, but none was thrown.")
    !exn

let throwsAsync<'T when 'T :> exn> (func: Async<obj>) = 
    Async.FromContinuations(fun (cont, econt, _) ->
        Async.StartWithContinuations(
            func,
            (fun _ -> econt(Exception("Expected an exception, but none was thrown."))),
            (fun e -> match (match e with :? AggregateException as e -> e.InnerException | _ -> e) with
                      | e when typeof<'T>.IsAssignableFrom(e.GetType()) -> 
                          cont (e :?> 'T)
                      | e ->
                          econt(Exception(sprintf "Expected an %A, got an %A (message: %s)" typeof<'T> (e.GetType()) e.Message))),
            (fun e -> if typeof<'T> <> typeof<OperationCanceledException> then
                          econt(Exception(sprintf "Expected an %A, got an OperationCanceledException." typeof<'T>))
                      else
                          cont (box e :?> 'T))
        )
    )

let run x = x |> Async.Ignore |> Async.RunSynchronously

let read<'T> prot =
    let thriftStruct = ThriftAttributesParser.ParseStruct(typeof<'T>.GetTypeInfo())
    ThriftStructReader.Read<'T>(thriftStruct, prot)

let write prot obj =
    let thriftStruct = ThriftAttributesParser.ParseStruct(obj.GetType().GetTypeInfo())
    let meth = typeof<ThriftStructWriter>.GetMethod("Write").MakeGenericMethod([| obj.GetType() |])
    try
        meth.Invoke(null, [| thriftStruct; obj; prot |]) |> ignore
    with
    | :? TargetInvocationException as e -> raise e.InnerException

let readMsgAsync<'T> prot name =
    let svc = ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())
    Thrift.CallMethodAsync<obj>({ new ThriftCommunication() with member x.CreateProtocol(_) = prot }, svc, name, [| |]) |> Async.AwaitTask


let writeMsgAsync<'T> methodName args = async {
    let m = MemoryProtocol([MessageHeader ("", ThriftMessageType.Reply)
                            StructHeader ""
                            FieldStop
                            StructEnd
                            MessageEnd])
    let svc = ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())
    do! Thrift.CallMethodAsync<obj>({ new ThriftCommunication() with member x.CreateProtocol(_) = m :> Protocols.IThriftProtocol }, svc, methodName, args) |> Async.AwaitTask |> Async.Ignore
    return m
}