namespace NativeMock.Emit
{
  using System;
  using System.Reflection;
  using System.Reflection.Emit;
  using Fluent;

  internal class DummyActionInterfaceMethodSelectorCodeGenerator : IDummyActionInterfaceMethodSelectorCodeGenerator
  {
    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    private static readonly MethodInfo s_getMethodFromHandleMethod
      = ReflectionInfoUtility.SelectMethod (() => MethodBase.GetMethodFromHandle (default!));

    private readonly ModuleBuilder _moduleBuilder;

    public DummyActionInterfaceMethodSelectorCodeGenerator (ModuleBuilder moduleBuilder)
    {
      if (moduleBuilder == null)
        throw new ArgumentNullException (nameof(moduleBuilder));

      _moduleBuilder = moduleBuilder;
    }

    /// <inheritdoc />
    public Type CreateDummyActionInterfaceMethodSelector (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type must be an interface.");

      var dummyActionControllerType = typeof(IDummyActionInterfaceMethodSelectorController);
      var dummyActionTypeBuilder = _moduleBuilder.DefinePublicClass ($"{interfaceType.Name}_DummyActionInterfaceMethodSelector_{Guid.NewGuid()}", typeof(object), interfaceType, dummyActionControllerType);

      var setCountField = dummyActionTypeBuilder.DefinePrivateField (typeof(int), "setCount");
      var resultField = dummyActionTypeBuilder.DefinePrivateField (typeof(MethodInfo), "result");

      // Implement the interface with stub selector methods
      foreach (var methodInfo in interfaceType.GetMethods())
        GenerateSelectorMethod (dummyActionTypeBuilder, methodInfo, setCountField, resultField);

      // Implement the controller interface
      GenerateProxyMetaMethods (dummyActionTypeBuilder, setCountField, resultField);

      try
      {
        return dummyActionTypeBuilder.CreateType() ?? throw new InvalidOperationException ("Could not create the requested proxy type.");
      }
      catch (TypeLoadException ex)
      {
        throw new InvalidOperationException (
          $"Could not create the requested proxy type. Make sure that the specified interface is publicly accessible or internal with an [assembly: InternalsVisibleTo(\"{NativeMockRegistry.ProxyAssemblyName}\")] attribute.",
          ex);
      }
    }

    private void GenerateSelectorMethod (TypeBuilder dummySelectorTypeBuilder, MethodInfo methodInfo, FieldBuilder setCountField, FieldBuilder resultField)
    {
      var returnType = methodInfo.ReturnType;

      var methodBuilder = dummySelectorTypeBuilder.DefineExplicitInterfaceMethodImplementation (methodInfo);
      var ilGenerator = methodBuilder.GetILGenerator();

      // setCount++;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, setCountField);
      ilGenerator.Emit (OpCodes.Ldc_I4_1);
      ilGenerator.Emit (OpCodes.Add);
      ilGenerator.Emit (OpCodes.Stfld, setCountField);

      // this.result = (MethodInfo) MethodInfo.GetMethodFromHandle(Interface.XXX);
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldtoken, methodInfo);
      ilGenerator.Emit (OpCodes.Call, s_getMethodFromHandleMethod);
      ilGenerator.Emit (OpCodes.Castclass, typeof(MethodInfo));
      ilGenerator.Emit (OpCodes.Stfld, resultField);

      // return;
      if (returnType == typeof(void))
      {
        // return;
        ilGenerator.Emit (OpCodes.Ret);
      }
      else if (returnType.IsValueType && !returnType.IsByRef)
      {
        // return default;
        var resultLocal = ilGenerator.DeclareLocal (returnType);
        ilGenerator.Emit (OpCodes.Ldloca_S, resultLocal);
        ilGenerator.Emit (OpCodes.Initobj, returnType);
        ilGenerator.Emit (OpCodes.Ldloc, resultLocal);
        ilGenerator.Emit (OpCodes.Ret);
      }
      else
      {
        // return null;
        ilGenerator.Emit (OpCodes.Ldnull);
        ilGenerator.Emit (OpCodes.Ret);
      }
    }

    private void GenerateProxyMetaMethods (TypeBuilder dummySelectorTypeBuilder, FieldBuilder setCountField, FieldBuilder resultField)
    {
      var dummyActionControllerType = typeof(IDummyActionInterfaceMethodSelectorController);

      // MethodInfo ? GetResult();
      var getResultInterfaceMethod = dummyActionControllerType.GetMethod (nameof(IDummyActionInterfaceMethodSelectorController.GetResult))!;
      var getResultMethodBuilder = dummySelectorTypeBuilder.DefineExplicitInterfaceMethodImplementation (getResultInterfaceMethod);
      GenerateSimpleGetMethod (getResultMethodBuilder, resultField);

      // int GetSetCount();
      var setCountInterfaceMethod = dummyActionControllerType.GetMethod (nameof(IDummyActionInterfaceMethodSelectorController.GetSetCount))!;
      var setCountMethodBuilder = dummySelectorTypeBuilder.DefineExplicitInterfaceMethodImplementation (setCountInterfaceMethod);
      GenerateSimpleGetMethod (setCountMethodBuilder, setCountField);

      // void Reset();
      var resetInterfaceMethod = dummyActionControllerType.GetMethod (nameof(IDummyActionInterfaceMethodSelectorController.Reset))!;
      var resetMethodBuilder = dummySelectorTypeBuilder.DefineExplicitInterfaceMethodImplementation (resetInterfaceMethod);
      GenerateResetMethod (resetMethodBuilder, setCountField, resultField);
    }

    private void GenerateSimpleGetMethod (MethodBuilder methodBuilder, FieldBuilder field)
    {
      var ilGenerator = methodBuilder.GetILGenerator();

      // return this.XXX;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, field);
      ilGenerator.Emit (OpCodes.Ret);
    }

    private void GenerateResetMethod (MethodBuilder methodBuilder, FieldBuilder setCountField, FieldBuilder resultField)
    {
      var ilGenerator = methodBuilder.GetILGenerator();

      // this.setCount = 0;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldc_I4_0);
      ilGenerator.Emit (OpCodes.Stfld, setCountField);

      // this.result = null;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldnull);
      ilGenerator.Emit (OpCodes.Stfld, resultField);

      // return;
      ilGenerator.Emit (OpCodes.Ret);
    }
  }
}
