namespace NativeMock.Emit
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;
  using Fluent;

  internal class DummyActionInterfaceMethodSelectorCodeGenerator : IDummyActionInterfaceMethodSelectorCodeGenerator
  {
    private const TypeAttributes c_classTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

    private const MethodAttributes c_implicitMethodImplementationAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
    private const MethodAttributes c_explicitMethodImplementationAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

    private const FieldAttributes c_instanceFieldAttributes = FieldAttributes.Private;

    private static readonly ConstructorInfo s_notSupportedExceptionConstructor = typeof(NotSupportedException).GetConstructor (Array.Empty<Type>())!;

    private static readonly MethodInfo s_getMethodFromHandleMethod = typeof(MethodBase).GetMethod (nameof(MethodBase.GetMethodFromHandle), new[] {typeof(RuntimeMethodHandle)})!;

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
      var dummyActionTypeBuilder = _moduleBuilder.DefineType ($"{interfaceType.Name}_DummyActionInterfaceMethodSelector_{Guid.NewGuid()}", c_classTypeAttributes, typeof(object), new[] {interfaceType, dummyActionControllerType});

      var setCountField = dummyActionTypeBuilder.DefineField ("setCount", typeof(int), c_instanceFieldAttributes);
      var resultField = dummyActionTypeBuilder.DefineField ("result", typeof(MethodInfo), c_instanceFieldAttributes);

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
        throw new InvalidOperationException ($"Could not create the requested proxy type. Make sure that the specified interface is publicly accessible or internal with an [assembly: InternalsVisibleTo(\"{NativeMockRegistry.ProxyAssemblyName}\")] attribute.", ex);
      }
    }

    private void GenerateSelectorMethod (TypeBuilder dummySelectorTypeBuilder, MethodInfo methodInfo, FieldBuilder setCountField, FieldBuilder resultField)
    {
      var returnType = methodInfo.ReturnType;
      var parameters = methodInfo.GetParameters().Select (e => e.ParameterType).ToArray();

      var methodBuilder = dummySelectorTypeBuilder.DefineMethod (methodInfo.Name, c_implicitMethodImplementationAttributes, returnType, parameters);
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
      if (returnType.IsByRef)
      {
        // We cannot return anything by ref here - there is no "default" value so we throw
        ilGenerator.Emit (OpCodes.Newobj, s_notSupportedExceptionConstructor);
        ilGenerator.Emit (OpCodes.Throw);
      }
      else if (returnType == typeof(void))
      {
        // return;
        ilGenerator.Emit (OpCodes.Ret);
      }
      else if (returnType.IsValueType)
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
      var getResultMethod = GenerateSimpleGetMethod (dummySelectorTypeBuilder, getResultInterfaceMethod.Name, resultField);
      dummySelectorTypeBuilder.DefineMethodOverride (getResultMethod, getResultInterfaceMethod);

      // int GetSetCount();
      var setCountInterfaceMethod = dummyActionControllerType.GetMethod (nameof(IDummyActionInterfaceMethodSelectorController.GetSetCount))!;
      var setCountMethod = GenerateSimpleGetMethod (dummySelectorTypeBuilder, setCountInterfaceMethod.Name, setCountField);
      dummySelectorTypeBuilder.DefineMethodOverride (setCountMethod, setCountInterfaceMethod);

      // void Reset();
      var resetInterfaceMethod = dummyActionControllerType.GetMethod (nameof(IDummyActionInterfaceMethodSelectorController.Reset))!;
      var resetMethod = GenerateResetMethod (dummySelectorTypeBuilder, setCountField, resultField);
      dummySelectorTypeBuilder.DefineMethodOverride (resetMethod, resetInterfaceMethod);
    }

    private MethodBuilder GenerateSimpleGetMethod (TypeBuilder dummySelectorTypeBuilder, string name, FieldBuilder field)
    {
      var getMethod = dummySelectorTypeBuilder.DefineMethod (
        $"{nameof(IDummyActionInterfaceMethodSelectorController)}.Get{name}",
        c_explicitMethodImplementationAttributes,
        field.FieldType,
        Array.Empty<Type>());

      var ilGenerator = getMethod.GetILGenerator();

      // return this.XXX;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, field);
      ilGenerator.Emit (OpCodes.Ret);

      return getMethod;
    }

    private MethodBuilder GenerateResetMethod (TypeBuilder dummySelectorTypeBuilder, FieldBuilder setCountField, FieldBuilder resultField)
    {
      var getMethod = dummySelectorTypeBuilder.DefineMethod (
        $"{nameof(IDummyActionInterfaceMethodSelectorController)}.Reset",
        c_explicitMethodImplementationAttributes,
        typeof(void),
        Array.Empty<Type>());

      var ilGenerator = getMethod.GetILGenerator();

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

      return getMethod;
    }
  }
}
