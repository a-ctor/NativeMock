namespace NativeMock.Emit
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
    private static readonly ConstructorInfo s_invalidOperationExceptionConstructor = typeof(NativeMockException).GetConstructor (new[] {typeof(string)})!;

    private static readonly MethodInfo s_getTypeMethod = typeof(object).GetMethod (nameof(GetType))!;
    private static readonly MethodInfo s_typeEqualsMethod = typeof(Type).GetMethod ("op_Equality", new[] {typeof(Type), typeof(Type)})!;

    private static readonly MethodInfo s_getTypeFromHandleMethod = typeof(Type).GetMethod ("GetTypeFromHandle")!;

    private static readonly MethodInfo s_delegateGetTargetMethod = typeof(Delegate).GetProperty (nameof(Delegate.Target))!.GetMethod!;
    private static readonly MethodInfo s_delegateGetMethodMethod = typeof(Delegate).GetProperty (nameof(Delegate.Method))!.GetMethod!;
    private static readonly MethodInfo s_createDelegateMethod = typeof(Delegate).GetMethod (nameof(Delegate.CreateDelegate), new[] {typeof(Type), typeof(object), typeof(MethodInfo)})!;

    private readonly ModuleBuilder _moduleBuilder;
    private readonly IDelegateFactory _delegateFactory;

    public NativeMockProxyCodeGenerator (ModuleBuilder moduleBuilder, IDelegateFactory delegateFactory)
    {
      if (moduleBuilder == null)
        throw new ArgumentNullException (nameof(moduleBuilder));
      if (delegateFactory == null)
        throw new ArgumentNullException (nameof(delegateFactory));

      _moduleBuilder = moduleBuilder;
      _delegateFactory = delegateFactory;
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
      var handlerFields = new FieldBuilder[interfaceMethods.Length];
      var callCountFields = new FieldBuilder[interfaceMethods.Length];

      for (var i = 0; i < interfaceMethods.Length; i++)
      {
        var (generatedMethod, handlerField, callCountField) = GenerateProxyMethod (proxyTypeBuilder, interfaceMethods[i], i + 1, underlyingImplementationField);
        generatedMethods.Add (generatedMethod);
        handlerFields[i] = handlerField;
        callCountFields[i] = callCountField;
      }

      // Implement the proxy interface
      GenerateProxyMetaMethods (proxyTypeBuilder, handlerFields, callCountFields, interfaceType, underlyingImplementationField);

      Type proxyType;
      try
      {
        proxyType = proxyTypeBuilder.CreateType() ?? throw new InvalidOperationException ("Could not create the requested proxy type.");
      }
      catch (TypeLoadException ex)
      {
        throw new InvalidOperationException (
          $"Could not create the requested proxy type. Make sure that the specified interface is publicly accessible or internal with an [assembly: InternalsVisibleTo(\"{NativeMockRegistry.ProxyAssemblyName}\")] attribute.",
          ex);
      }

      return new NativeMockProxyCodeGeneratorResult (proxyType, generatedMethods.MoveToImmutable());
    }

    private (NativeMockProxyCodeGeneratedMethod generatedMethod, FieldBuilder handerField, FieldBuilder callCountField) GenerateProxyMethod (
      TypeBuilder proxyTypeBuilder,
      MethodInfo methodInfo,
      int methodHandle,
      FieldBuilder underlyingImplementationField)
    {
      var returnType = methodInfo.ReturnType;
      var parameters = methodInfo.GetParameters().Select (e => e.ParameterType).ToArray();

      var delegateType = _delegateFactory.CreateDelegateType (methodInfo);
      var delegateInvokeMethod = delegateType.GetMethod ("Invoke")!;

      // Instance field containing the handler
      var handlerFieldBuilder = proxyTypeBuilder.DefineField ($"handler{methodHandle}", delegateType, c_instanceFieldAttributes);
      var callCountFieldBuilder = proxyTypeBuilder.DefineField ($"callCount{methodHandle}", typeof(int), c_instanceFieldAttributes);

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
      ilGenerator.Emit (OpCodes.Ldstr, $"'{methodInfo.DeclaringType!.Name}.{methodInfo.Name}' invocation failed because no setup was found.");
      ilGenerator.Emit (OpCodes.Newobj, s_invalidOperationExceptionConstructor);
      ilGenerator.Emit (OpCodes.Throw);

      // callHandler:
      ilGenerator.MarkLabel (callHandlerLabel);

      // this.callCountX++;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Dup);
      ilGenerator.Emit (OpCodes.Ldfld, callCountFieldBuilder);
      ilGenerator.Emit (OpCodes.Ldc_I4_1);
      ilGenerator.Emit (OpCodes.Add);
      ilGenerator.Emit (OpCodes.Stfld, callCountFieldBuilder);

      // return this.handlerX(...args)
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
      return (nativeMockProxyCodeGeneratedMethod, handlerFieldBuilder, callCountFieldBuilder);
    }

    private void GenerateProxyMetaMethods (
      TypeBuilder proxyTypeBuilder,
      FieldBuilder[] handlerFields,
      FieldBuilder[] callCountFields,
      Type interfaceType,
      FieldBuilder underlyingImplementationField)
    {
      var proxyControllerType = typeof(INativeMockProxyController<>).MakeGenericType (interfaceType);

      // GetMethodCount ()
      var getMethodHandlerCountInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.GetMethodCount))!;
      var getMethodHandlerCountMethod = GenerateGetMethodHandlerCount (proxyTypeBuilder, handlerFields.Length, interfaceType);
      proxyTypeBuilder.DefineMethodOverride (getMethodHandlerCountMethod, getMethodHandlerCountInterfaceMethod);

      // GetMethodHandlerCallCount (int methodHandle, Delegate handler)
      var getMethodHandlerCallCountInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.GetMethodHandlerCallCount))!;
      var getMethodHandlerCallCountMethod = GenerateGetMethod (proxyTypeBuilder, callCountFields, interfaceType, typeof(int));
      proxyTypeBuilder.DefineMethodOverride (getMethodHandlerCallCountMethod, getMethodHandlerCallCountInterfaceMethod);

      // GetMethodHandler (int methodHandle)
      var getMethodHandlerInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.GetMethodHandler))!;
      var getMethodHandlerMethod = GenerateGetMethod (proxyTypeBuilder, handlerFields, interfaceType, typeof(Delegate));
      proxyTypeBuilder.DefineMethodOverride (getMethodHandlerMethod, getMethodHandlerInterfaceMethod);

      // SetHandler (int methodHandle, Delegate handler)
      var setHandlerInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.SetMethodHandler))!;
      var setHandlerMethod = GenerateSetHandlerMethod (proxyTypeBuilder, handlerFields, interfaceType);
      proxyTypeBuilder.DefineMethodOverride (setHandlerMethod, setHandlerInterfaceMethod);

      // void SetUnderlyingImplementation (T underlyingImplementation);
      var setUnderlyingImplementationInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.SetUnderlyingImplementation))!;
      var setUnderlyingImplementationMethod = GenerateSetUnderlyingImplementation (proxyTypeBuilder, interfaceType, underlyingImplementationField);
      proxyTypeBuilder.DefineMethodOverride (setUnderlyingImplementationMethod, setUnderlyingImplementationInterfaceMethod);
    }

    private MethodBuilder GenerateGetMethodHandlerCount (TypeBuilder proxyTypeBuilder, int methodCount, Type interfaceType)
    {
      var getMethodHandlerCount = proxyTypeBuilder.DefineMethod (
        $"{typeof(INativeMockProxyController<>).Name}<{interfaceType.Name}>.{nameof(INativeMockProxyController<object>.GetMethodCount)}",
        c_explicitMethodImplementationAttributes,
        typeof(int),
        Array.Empty<Type>());

      var ilGenerator = getMethodHandlerCount.GetILGenerator();

      // return methodCount;
      ilGenerator.Emit (OpCodes.Ldc_I4, methodCount);
      ilGenerator.Emit (OpCodes.Ret);

      return getMethodHandlerCount;
    }

    private MethodBuilder GenerateGetMethod (
      TypeBuilder proxyTypeBuilder,
      FieldBuilder[] generatedMethodFields,
      Type interfaceType,
      Type returnType)
    {
      var getMethodHandlerCallCount = proxyTypeBuilder.DefineMethod (
        $"{typeof(INativeMockProxyController<>).Name}<{interfaceType.Name}>.{nameof(INativeMockProxyController<object>.GetMethodHandlerCallCount)}",
        c_explicitMethodImplementationAttributes,
        returnType,
        new[] {typeof(int)});

      var ilGenerator = getMethodHandlerCallCount.GetILGenerator();

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

        // case X:
        ilGenerator.MarkLabel (switchLabels[i]);

        // return this.callCountX
        ilGenerator.Emit (OpCodes.Ldarg_0);
        ilGenerator.Emit (OpCodes.Ldfld, field);
        ilGenerator.Emit (OpCodes.Ret);
      }
      // }

      return getMethodHandlerCallCount;
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

        var convertDelegateLabel = ilGenerator.DefineLabel();

        // case X:
        ilGenerator.MarkLabel (switchLabels[i]);

        // if (typeof(X) == arg.GetType()) {
        ilGenerator.Emit (OpCodes.Ldtoken, delegateType);
        ilGenerator.Emit (OpCodes.Call, s_getTypeFromHandleMethod);

        ilGenerator.Emit (OpCodes.Ldarg_2);
        ilGenerator.Emit (OpCodes.Callvirt, s_getTypeMethod);

        ilGenerator.Emit (OpCodes.Call, s_typeEqualsMethod);
        ilGenerator.Emit (OpCodes.Brfalse_S, convertDelegateLabel);

        ilGenerator.Emit (OpCodes.Ldarg_0);
        ilGenerator.Emit (OpCodes.Ldarg_2);
        ilGenerator.Emit (OpCodes.Castclass, delegateType);
        ilGenerator.Emit (OpCodes.Stfld, field);
        ilGenerator.Emit (OpCodes.Ret);

        // }

        ilGenerator.MarkLabel (convertDelegateLabel);

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
