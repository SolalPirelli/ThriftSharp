// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

[<AutoOpen>]
module ThriftSharp.Tests.Utils

open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open System.Threading
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.VisualStudio.TestTools.UnitTesting
open Linq.QuotationEvaluation
open ThriftSharp
open ThriftSharp.Internals

// Shorter name
type Test = TestMethodAttribute
// So other files don't have to reference the VS testing namespace
type TestClass = TestClassAttribute

let tid (n: int) = byte n |> LanguagePrimitives.EnumOfValue

let date(dd, mm, yyyy) = 
    let d = DateTime(yyyy, mm, dd, 00, 00, 00, DateTimeKind.Utc)
    d.ToLocalTime()

// Use Nullable without having to open System (because of conflicts with e.g. Int32 in memory protocol) or use the long notation
let nullable x = Nullable(x)

let dict (vals: ('a * 'b) seq) =
    let dic = Dictionary()
    for (k,v) in vals do
        dic.Add(k, v)
    dic

let rec private eq (act: obj) (exp: obj) = // can safely assume act and exp are of the same type
    if act = null then
        exp = null
    elif act :? IDictionary then
        let act = act :?> IDictionary
        let exp = exp :?> IDictionary
        eq act.Keys exp.Keys && eq act.Values exp.Values
    elif not (act :? string) && act :? IEnumerable then
        let act = (act :?> IEnumerable).GetEnumerator()
        let exp = (exp :?> IEnumerable).GetEnumerator()
        
        let rec eqEnum (act: IEnumerator) (exp: IEnumerator) =
            if act.MoveNext() then exp.MoveNext() && eq act.Current exp.Current && eqEnum act exp
            else not (exp.MoveNext())

        eqEnum act exp
    elif FSharpType.IsUnion(act.GetType()) || act.GetType().IsEnum then
        act = exp
    // HACK
    elif act.GetType().Assembly.FullName.Contains("ThriftSharp") then
        act.GetType().GetRuntimeProperties()
     |> Seq.filter (fun p -> p.Name = "Message" || p.DeclaringType = act.GetType())
     |> Seq.forall (fun p -> eq (p.GetValue(act)) (p.GetValue(exp)))
    else
        Object.Equals(exp, act)

/// Ensures both objects are equal, comparing for collection, reference or structural equality
let (<=>) (act: 'a) (exp: 'a) =
    if not (eq act exp) then Assert.Fail(sprintf "Expected: %A%sActual: %A" exp Environment.NewLine act)

let throws<'T when 'T :> exn> func =
    let exn = ref Unchecked.defaultof<'T>

    try
        func() |> ignore
    with
    | ex when typeof<'T>.IsAssignableFrom(ex.GetType()) -> 
        exn := ex :?> 'T
    | ex -> 
        Assert.Fail(sprintf "Expected an exception of type %A, but got one of type %A (message: %s)" typeof<'T> (ex.GetType()) ex.Message)
    
    if Object.Equals(!exn, null) then
        Assert.Fail("Expected an exception, but none was thrown.")
    !exn

let throwsAsync<'T when 'T :> exn> (func: Async<obj>) = 
    Async.FromContinuations(fun (cont, econt, _) ->
        Async.StartWithContinuations(
            func,
            (fun _ -> econt(AssertFailedException("Expected an exception, but none was thrown."))),
            (fun e -> match (match e with :? AggregateException as e -> e.InnerException | _ -> e) with
                      | e when typeof<'T>.IsAssignableFrom(e.GetType()) -> 
                          cont (e :?> 'T)
                      | e ->
                          econt(AssertFailedException(sprintf "Expected an %A, got an %A (message: %s)" typeof<'T> (e.GetType()) e.Message))),
            (fun e -> if typeof<'T> <> typeof<OperationCanceledException> then
                          econt(AssertFailedException(sprintf "Expected an %A, got an OperationCanceledException." typeof<'T>))
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
    Thrift.CallMethodAsync<obj>(ThriftCommunication(prot), svc, name, [| |]) |> Async.AwaitTask


let writeMsgAsync<'T> methodName args = async {
    let m = MemoryProtocol([MessageHeader ("", ThriftMessageType.Reply)
                            StructHeader ""
                            FieldStop
                            StructEnd
                            MessageEnd])
    let svc = ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())
    do! Thrift.CallMethodAsync<obj>(ThriftCommunication(m), svc, methodName, args) |> Async.AwaitTask |> Async.Ignore
    return m
}

let makeClass structAttrs propsAndAttrs =
    let ctorAndArgs = function
        | NewObject (ctor, args) -> ctor, (args |> Array.ofList |> Array.map (fun a -> a.EvalUntyped()))
        | _ -> failwith "not a ctor and args"

    let guid = Guid.NewGuid()
    let assemblyName = AssemblyName(guid.ToString())
    let moduleBuilder = Thread.GetDomain()
                              .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                              .DefineDynamicModule(assemblyName.Name)
    
    let typeBuilder = moduleBuilder.DefineType("GeneratedType", TypeAttributes.Class ||| TypeAttributes.Public)
    
    for expr in structAttrs do
        let (ctor, args) = ctorAndArgs expr
        typeBuilder.SetCustomAttribute(CustomAttributeBuilder(ctor, args))

    for (name, typ, attrExprs) in propsAndAttrs do
        // backing field
        let fieldBuilder = typeBuilder.DefineField("_" + name, typ, FieldAttributes.Private)

        // getter
        let getterBuilder = typeBuilder.DefineMethod("get_" + name, MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig, typ, Type.EmptyTypes)
        let getterIL = getterBuilder.GetILGenerator()
        getterIL.Emit(OpCodes.Ldarg_0)
        getterIL.Emit(OpCodes.Ldfld, fieldBuilder)
        getterIL.Emit(OpCodes.Ret)
        
        // setter
        let setterBuilder = typeBuilder.DefineMethod("set_" + name, MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig, null, [| typ |])
        let setterIL = setterBuilder.GetILGenerator()
        setterIL.Emit(OpCodes.Ldarg_0)
        setterIL.Emit(OpCodes.Ldarg_1)
        setterIL.Emit(OpCodes.Stfld, fieldBuilder)
        setterIL.Emit(OpCodes.Ret)

        // property
        let propBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, typ, null)
        propBuilder.SetGetMethod(getterBuilder)
        propBuilder.SetSetMethod(setterBuilder)

        // attributes
        for expr in attrExprs do
            let (ctor, args) = ctorAndArgs expr
            let attrBuilder = CustomAttributeBuilder(ctor, args)
            propBuilder.SetCustomAttribute(attrBuilder)

    typeBuilder.CreateType()

let makeInterface interfaceAttrs methodsAndAttrs =    
    let ctorAndArgs = function
        | NewObject (ctor, args) -> ctor, (args |> Array.ofList |> Array.map (fun a -> a.EvalUntyped()))
        | _ -> failwith "not a ctor and args"

    let guid = Guid.NewGuid()
    let assemblyName = AssemblyName(guid.ToString())
    let moduleBuilder = Thread.GetDomain()
                              .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                              .DefineDynamicModule(assemblyName.Name)
    
    let interfaceBuilder = moduleBuilder.DefineType("GeneratedType", TypeAttributes.Interface ||| TypeAttributes.Abstract ||| TypeAttributes.Public)
        
    for expr in interfaceAttrs do
        let (ctor, args) = ctorAndArgs expr
        interfaceBuilder.SetCustomAttribute(CustomAttributeBuilder(ctor, args))

    methodsAndAttrs |> Seq.iteri (fun n (args, retType, attrExprs) ->
        let methodBuilder = interfaceBuilder.DefineMethod(string n, MethodAttributes.Public ||| MethodAttributes.Abstract ||| MethodAttributes.Virtual, retType, args |> Array.map fst)

        args |> Seq.iteri (fun i (_, argAttrExprs) ->
            let paramBuilder = methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, string i) // +1 because 0 is the return value
            
            for expr in argAttrExprs do
                let (ctor, args) = ctorAndArgs expr
                paramBuilder.SetCustomAttribute(CustomAttributeBuilder(ctor, args))
            )

        for expr in attrExprs do
            let (ctor, args) = ctorAndArgs expr
            methodBuilder.SetCustomAttribute(CustomAttributeBuilder(ctor, args))
        )
    
    interfaceBuilder.CreateType()