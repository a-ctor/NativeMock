namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;

  internal class NativeMockProxyCodeGenerator : INativeMockProxyCodeGenerator
  {
    private const TypeAttributes c_classTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

    private const MethodAttributes c_implicitMethodImplementationAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
    private const MethodAttributes c_explicitMethodImplementationAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

    private const FieldAttributes c_instanceFieldAttributes = FieldAttributes.Private;

    private static readonly ConstructorInfo s_argumentExceptionConstructor = typeof(ArgumentException).GetConstructor (new[] {typeof(string), typeof(string)})!;
    private static readonly ConstructorInfo s_invalidOperationExceptionConstructor = typeof(InvalidOperationException).GetConstructor (new[] {typeof(string)})!;

    private static readonly MethodInfo s_getTypeFromHandleMethod = typeof(Type).GetMethod ("GetTypeFromHandle")!;

    private static readonly MethodInfo s_delegateGetTargetMethod = typeof(Delegate).GetProperty (nameof(Delegate.Target))!.GetMethod!;
    private static readonly MethodInfo s_delegateGetMethodMethod = typeof(Delegate).GetProperty (nameof(Delegate.Method))!.GetMethod!;
    private static readonly MethodInfo s_createDelegateMethod = typeof(Delegate).GetMethod (nameof(Delegate.CreateDelegate), new[] {typeof(Type), typeof(object), typeof(MethodInfo)})!;

    private static readonly MethodInfo s_proxySetHandlerMethod = typeof(INativeMockProxyController).GetMethod (nameof(INativeMockProxyController.SetMethodHandler))!;

    private readonly ModuleBuilder _moduleBuilder;
    private readonly DelegateGenerator _delegateGenerator;

    public NativeMockProxyCodeGenerator (ModuleBuilder moduleBuilder, DelegateGenerator delegateGenerator)
    {
      if (moduleBuilder == null)
        throw new ArgumentNullException (nameof(moduleBuilder));
      if (delegateGenerator == null)
        throw new ArgumentNullException (nameof(delegateGenerator));

      _moduleBuilder = moduleBuilder;
      _delegateGenerator = new DelegateGenerator (_moduleBuilder);
    }

    /// <inheritdoc />
    public NativeMockProxyCodeGeneratorResult CreateProxy (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type must be an interface.");

      var proxyTypeBuilder = _moduleBuilder.DefineType ($"{interfaceType.Name}_NativeFunctionProxyDelegate_{Guid.NewGuid()}", c_classTypeAttributes, typeof(object), new[] {interfaceType, typeof(INativeMockProxyController)});

      // Implement the interface that we want to proxy
      var interfaceMethods = interfaceType.GetMethods();
      var generatedMethods = ImmutableArray.CreateBuilder<NativeMockProxyCodeGeneratedMethod> (interfaceMethods.Length);
      var generatedFields = new FieldBuilder[interfaceMethods.Length];

      for (var i = 0; i < interfaceMethods.Length; i++)
      {
        var (generatedMethod, field) = GenerateProxyMethod (proxyTypeBuilder, interfaceMethods[i], i + 1);
        generatedMethods.Add (generatedMethod);
        generatedFields[i] = field;
      }

      // Implement the proxy interface
      GenerateProxyMetaMethods (proxyTypeBuilder, generatedFields);

      // todo: do accessibility check and throw if inaccessible
      var proxyType = proxyTypeBuilder.CreateType();
      if (proxyType == null)
        throw new InvalidOperationException ("Could not create the requested proxy type.");

      return new NativeMockProxyCodeGeneratorResult (proxyType, generatedMethods.MoveToImmutable());
    }

    private (NativeMockProxyCodeGeneratedMethod generatedMethod, FieldBuilder field) GenerateProxyMethod (TypeBuilder proxyTypeBuilder, MethodInfo methodInfo, int methodHandle)
    {
      var returnType = methodInfo.ReturnType;
      var parameters = methodInfo.GetParameters().Select (e => e.ParameterType).ToArray();

      var delegateType = _delegateGenerator.CreateDelegateType (methodInfo);
      var delegateInvokeMethod = delegateType.GetMethod ("Invoke")!;

      // Instance field containing the handler
      var handlerFieldBuilder = proxyTypeBuilder.DefineField ($"field{methodHandle}", delegateType, c_instanceFieldAttributes);

      var methodBuilder = proxyTypeBuilder.DefineMethod (methodInfo.Name, c_implicitMethodImplementationAttributes, returnType, parameters);
      var ilGenerator = methodBuilder.GetILGenerator();

      var callHandlerLabel = ilGenerator.DefineLabel();

      // this.fieldX
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, handlerFieldBuilder);

      // if (this.fieldX == null) {
      ilGenerator.Emit (OpCodes.Dup);
      ilGenerator.Emit (OpCodes.Brtrue_S, callHandlerLabel);

      // throw new InvalidOperationException("
      ilGenerator.Emit (OpCodes.Ldstr, "No handler was set up."); // todo better exception
      ilGenerator.Emit (OpCodes.Newobj, s_invalidOperationExceptionConstructor);
      ilGenerator.Emit (OpCodes.Throw);
      // }
      ilGenerator.MarkLabel (callHandlerLabel);

      // return this.fieldX(...args)
      for (short i = 1; i <= parameters.Length; i++)
        ilGenerator.Emit (OpCodes.Ldarg, i);
      ilGenerator.Emit (OpCodes.Callvirt, delegateInvokeMethod);
      ilGenerator.Emit (OpCodes.Ret);

      var nativeMockProxyCodeGeneratedMethod = new NativeMockProxyCodeGeneratedMethod (methodInfo, methodHandle);
      return (nativeMockProxyCodeGeneratedMethod, handlerFieldBuilder);
    }

    private void GenerateProxyMetaMethods (TypeBuilder proxyTypeBuilder, FieldBuilder[] generatedMethodFields)
    {
      // SetHandler (int methodHandle, Delegate handler)
      var setHandlerMethod = proxyTypeBuilder.DefineMethod (
        $"{nameof(INativeMockProxyController)}.{nameof(INativeMockProxyController.SetMethodHandler)}",
        c_explicitMethodImplementationAttributes,
        typeof(void),
        new[] {typeof(int), typeof(Delegate)});

      var ilGenerator = setHandlerMethod.GetILGenerator();

      Label[] switchLabels = new Label[generatedMethodFields.Length];
      for (var i = 0; i < switchLabels.Length; i++)
        switchLabels[i] = ilGenerator.DefineLabel();

      // switch(methodHandle) {
      ilGenerator.Emit (OpCodes.Ldarg_1);
      ilGenerator.Emit (OpCodes.Ldc_I4_1);
      ilGenerator.Emit (OpCodes.Sub);
      ilGenerator.Emit (OpCodes.Switch, switchLabels);

      // default: throw new ArgumentException("Invalid method handle specified", nameof(methodHandle));
      ilGenerator.Emit (OpCodes.Ldstr, "Invalid method handle specified");
      ilGenerator.Emit (OpCodes.Ldstr, "methodHandle");
      ilGenerator.Emit (OpCodes.Newobj, s_argumentExceptionConstructor);
      ilGenerator.Emit (OpCodes.Throw);

      for (var i = 0; i < switchLabels.Length; i++)
      {
        var field = generatedMethodFields[i];
        var delegateType = field.FieldType;

        // case X:
        ilGenerator.MarkLabel (switchLabels[i]);

        // this.fieldX = ...
        ilGenerator.Emit (OpCodes.Ldarg_0);

        // ... (X) Delegate.CreateDelegate(typeof(X), methodHandle.Target, methodHandle.Method);
        ilGenerator.Emit (OpCodes.Ldtoken, delegateType);
        ilGenerator.Emit (OpCodes.Call, s_getTypeFromHandleMethod);

        ilGenerator.Emit (OpCodes.Ldarg_2);
        ilGenerator.Emit (OpCodes.Callvirt, s_delegateGetTargetMethod);

        ilGenerator.Emit (OpCodes.Ldarg_2);
        ilGenerator.Emit (OpCodes.Callvirt, s_delegateGetMethodMethod);

        ilGenerator.Emit (OpCodes.Call, s_createDelegateMethod);
        ilGenerator.Emit (OpCodes.Castclass, delegateType);

        ilGenerator.Emit (OpCodes.Stfld, field);

        // return; // break;
        ilGenerator.Emit (OpCodes.Ret);
      }
      // }

      proxyTypeBuilder.DefineMethodOverride (setHandlerMethod, s_proxySetHandlerMethod);
    }
  }
}
