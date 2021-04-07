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

    private readonly ModuleBuilder _moduleBuilder;
    private readonly IDelegateCodeGenerator _delegateCodeGenerator;

    public NativeMockProxyCodeGenerator (ModuleBuilder moduleBuilder, IDelegateCodeGenerator delegateCodeGenerator)
    {
      if (moduleBuilder == null)
        throw new ArgumentNullException (nameof(moduleBuilder));
      if (delegateCodeGenerator == null)
        throw new ArgumentNullException (nameof(delegateCodeGenerator));

      _moduleBuilder = moduleBuilder;
      _delegateCodeGenerator = delegateCodeGenerator;
    }

    /// <inheritdoc />
    public NativeMockProxyCodeGeneratorResult CreateProxy (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type must be an interface.");

      var proxyControllerType = typeof(INativeMockProxyController<>).MakeGenericType (interfaceType);
      var proxyTypeBuilder = _moduleBuilder.DefineType ($"{interfaceType.Name}_NativeFunctionProxyDelegate_{Guid.NewGuid()}", c_classTypeAttributes, typeof(object), new[] {interfaceType, proxyControllerType});

      var underlyingImplementationField = proxyTypeBuilder.DefineField ("underlyingImplementation", interfaceType, FieldAttributes.Private);

      // Implement the interface that we want to proxy
      var interfaceMethods = interfaceType.GetMethods();
      var generatedMethods = ImmutableArray.CreateBuilder<NativeMockProxyCodeGeneratedMethod> (interfaceMethods.Length);
      var generatedFields = new FieldBuilder[interfaceMethods.Length];

      for (var i = 0; i < interfaceMethods.Length; i++)
      {
        var (generatedMethod, field) = GenerateProxyMethod (proxyTypeBuilder, interfaceMethods[i], i + 1, underlyingImplementationField);
        generatedMethods.Add (generatedMethod);
        generatedFields[i] = field;
      }

      // Implement the proxy interface
      GenerateProxyMetaMethods (proxyTypeBuilder, generatedFields, interfaceType, underlyingImplementationField);

      // todo: do accessibility check and throw if inaccessible
      var proxyType = proxyTypeBuilder.CreateType();
      if (proxyType == null)
        throw new InvalidOperationException ("Could not create the requested proxy type.");

      return new NativeMockProxyCodeGeneratorResult (proxyType, generatedMethods.MoveToImmutable());
    }

    private (NativeMockProxyCodeGeneratedMethod generatedMethod, FieldBuilder field) GenerateProxyMethod (TypeBuilder proxyTypeBuilder, MethodInfo methodInfo, int methodHandle, FieldBuilder underlyingImplementationField)
    {
      var returnType = methodInfo.ReturnType;
      var parameters = methodInfo.GetParameters().Select (e => e.ParameterType).ToArray();

      var delegateType = _delegateCodeGenerator.CreateDelegateType (methodInfo);
      var delegateInvokeMethod = delegateType.GetMethod ("Invoke")!;

      // Instance field containing the handler
      var handlerFieldBuilder = proxyTypeBuilder.DefineField ($"field{methodHandle}", delegateType, c_instanceFieldAttributes);

      var methodBuilder = proxyTypeBuilder.DefineMethod (methodInfo.Name, c_implicitMethodImplementationAttributes, returnType, parameters);
      var ilGenerator = methodBuilder.GetILGenerator();

      var callHandlerLabel = ilGenerator.DefineLabel();
      var callUnderlyingImplementation = ilGenerator.DefineLabel();

      // this.fieldX
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, handlerFieldBuilder);

      // if (this.fieldX != null) goto callHandler;
      ilGenerator.Emit (OpCodes.Dup);
      ilGenerator.Emit (OpCodes.Brtrue_S, callHandlerLabel);
      ilGenerator.Emit (OpCodes.Pop);

      // this.underlyingImplementation
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, underlyingImplementationField);

      // if (this.underlyingImplementation != null) goto callUnderlyingImplementation;
      ilGenerator.Emit (OpCodes.Dup);
      ilGenerator.Emit (OpCodes.Brtrue_S, callUnderlyingImplementation);
      ilGenerator.Emit (OpCodes.Pop);

      // throw new InvalidOperationException("
      ilGenerator.Emit (OpCodes.Ldstr, "No handler was set up."); // todo better exception
      ilGenerator.Emit (OpCodes.Newobj, s_invalidOperationExceptionConstructor);
      ilGenerator.Emit (OpCodes.Throw);

      // callHandler:
      ilGenerator.MarkLabel (callHandlerLabel);

      // return this.fieldX(...args)
      for (short i = 1; i <= parameters.Length; i++)
        ilGenerator.Emit (OpCodes.Ldarg, i);
      ilGenerator.Emit (OpCodes.Callvirt, delegateInvokeMethod);
      ilGenerator.Emit (OpCodes.Ret);

      // callUnderlyingImplementation:
      ilGenerator.MarkLabel (callUnderlyingImplementation);

      // return this.underlyingImplementation.XXX(...args)
      for (short i = 1; i <= parameters.Length; i++)
        ilGenerator.Emit (OpCodes.Ldarg, i);
      ilGenerator.Emit (OpCodes.Callvirt, methodInfo);
      ilGenerator.Emit (OpCodes.Ret);

      var nativeMockProxyCodeGeneratedMethod = new NativeMockProxyCodeGeneratedMethod (methodInfo, methodHandle);
      return (nativeMockProxyCodeGeneratedMethod, handlerFieldBuilder);
    }

    private void GenerateProxyMetaMethods (TypeBuilder proxyTypeBuilder, FieldBuilder[] generatedMethodFields, Type interfaceType, FieldBuilder underlyingImplementationField)
    {
      var proxyControllerType = typeof(INativeMockProxyController<>).MakeGenericType (interfaceType);

      // SetHandler (int methodHandle, Delegate handler)
      var setHandlerInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.SetMethodHandler))!;
      var setHandlerMethod = GenerateSetHandlerMethod (proxyTypeBuilder, generatedMethodFields, interfaceType);
      proxyTypeBuilder.DefineMethodOverride (setHandlerMethod, setHandlerInterfaceMethod);

      // void SetUnderlyingImplementation (T underlyingImplementation);
      var setUnderlyingImplementationInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.SetUnderlyingImplementation))!;
      var setUnderlyingImplementationMethod = GenerateSetUnderlyingImplementation (proxyTypeBuilder, interfaceType, underlyingImplementationField);
      proxyTypeBuilder.DefineMethodOverride (setUnderlyingImplementationMethod, setUnderlyingImplementationInterfaceMethod);
    }

    private MethodBuilder GenerateSetHandlerMethod (TypeBuilder proxyTypeBuilder, FieldBuilder[] generatedMethodFields, Type interfaceType)
    {
      var setHandlerMethod = proxyTypeBuilder.DefineMethod (
        $"{typeof(INativeMockProxyController<>).Name}<{interfaceType.Name}>.{nameof(INativeMockProxyController<object>.SetMethodHandler)}",
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

      return setHandlerMethod;
    }

    private MethodBuilder GenerateSetUnderlyingImplementation (TypeBuilder proxyTypeBuilder, Type interfaceType, FieldBuilder underlyingImplementationField)
    {
      var setUnderlyingImplementationMethod = proxyTypeBuilder.DefineMethod (
        $"{typeof(INativeMockProxyController<>).Name}<{interfaceType.Name}>.{nameof(INativeMockProxyController<object>.SetUnderlyingImplementation)}",
        c_explicitMethodImplementationAttributes,
        typeof(void),
        new[] {interfaceType});

      var ilGenerator = setUnderlyingImplementationMethod.GetILGenerator();
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldarg_1);
      ilGenerator.Emit (OpCodes.Stfld, underlyingImplementationField);
      ilGenerator.Emit (OpCodes.Ret);

      return setUnderlyingImplementationMethod;
    }
  }
}
