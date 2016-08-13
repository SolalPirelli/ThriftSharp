// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

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


/// Easily create IDictionary instances
let dict vals =
    let dic = Dictionary()
    Seq.iter dic.Add vals
    dic


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
let asTask a =
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