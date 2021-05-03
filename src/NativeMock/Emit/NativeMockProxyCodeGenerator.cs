namespace NativeMock.Emit
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;
  using System.Threading;

  internal class NativeMockProxyCodeGenerator : INativeMockProxyCodeGenerator
  {
    private struct GeneratedProxyMethodResult
    {
      public readonly NativeMockProxyCodeGeneratedMethod GeneratedMethod;
      public readonly FieldBuilder HanderField;
      public readonly FieldBuilder CallCountField;

      public GeneratedProxyMethodResult (NativeMockProxyCodeGeneratedMethod generatedMethod, FieldBuilder handerField, FieldBuilder callCountField)
      {
        GeneratedMethod = generatedMethod;
        HanderField = handerField;
        CallCountField = callCountField;
      }
    }

    private static readonly ConstructorInfo s_argumentExceptionConstructor
      = ReflectionInfoUtility.SelectConstructor (() => new ArgumentException ("", ""));

    private static readonly ConstructorInfo s_nativeMockExceptionConstructor
      = ReflectionInfoUtility.SelectConstructor (() => new NativeMockException (""));

    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    private static readonly MethodInfo s_getTypeMethod = ReflectionInfoUtility.SelectMethod<object> (e => e.GetType());
    private static readonly MethodInfo s_typeEqualsMethod = ReflectionInfoUtility.GetEqualityOperatorMethod (typeof(Type), typeof(Type), typeof(Type));

    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    private static readonly MethodInfo s_getTypeFromHandleMethod = ReflectionInfoUtility.SelectMethod (() => Type.GetTypeFromHandle (default));

    private static readonly MethodInfo s_delegateGetTargetMethod = ReflectionInfoUtility.SelectGetter<Delegate> (e => e.Target);
    private static readonly MethodInfo s_delegateGetMethodMethod = ReflectionInfoUtility.SelectGetter<Delegate> (e => e.Method);
    private static readonly MethodInfo s_createDelegateMethod = ReflectionInfoUtility.SelectMethod (() => Delegate.CreateDelegate (null!, default, ((MethodInfo?) null)!));

    private static readonly MethodInfo s_interlockedIntIncrement = typeof(Interlocked).GetMethod (nameof(Interlocked.Increment), new[] {typeof(int).MakeByRefType()})!;

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
      var proxyTypeBuilder = _moduleBuilder.DefinePublicClass ($"{interfaceType.Name}_NativeFunctionProxyDelegate_{Guid.NewGuid()}", typeof(object), interfaceType, proxyControllerType);

      var underlyingImplementationField = proxyTypeBuilder.DefineField ("underlyingImplementation", interfaceType, FieldAttributes.Private);

      // Implement the interface that we want to proxy
      var interfaceMethods = interfaceType.GetMethods();
      var generatedMethods = ImmutableArray.CreateBuilder<NativeMockProxyCodeGeneratedMethod> (interfaceMethods.Length);
      var handlerFields = new FieldBuilder[interfaceMethods.Length];
      var callCountFields = new FieldBuilder[interfaceMethods.Length];

      for (var i = 0; i < interfaceMethods.Length; i++)
      {
        var result = GenerateProxyMethod (proxyTypeBuilder, interfaceMethods[i], i + 1, underlyingImplementationField);
        generatedMethods.Add (result.GeneratedMethod);
        handlerFields[i] = result.HanderField;
        callCountFields[i] = result.CallCountField;
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

    private GeneratedProxyMethodResult GenerateProxyMethod (
      TypeBuilder proxyTypeBuilder,
      MethodInfo methodInfo,
      int methodHandle,
      FieldBuilder underlyingImplementationField)
    {
      var parameters = methodInfo.GetParameters().ToArray();

      var delegateType = _delegateFactory.CreateDelegateType (methodInfo);
      var delegateInvokeMethod = delegateType.GetMethod ("Invoke")!;

      // Instance field containing the handler
      var handlerFieldBuilder = proxyTypeBuilder.DefinePrivateField (delegateType, $"handler{methodHandle}");
      var callCountFieldBuilder = proxyTypeBuilder.DefinePrivateField (typeof(int), $"callCount{methodHandle}");

      var methodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (methodInfo);
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
      ilGenerator.Emit (OpCodes.Newobj, s_nativeMockExceptionConstructor);
      ilGenerator.Emit (OpCodes.Throw);

      // callHandler:
      ilGenerator.MarkLabel (callHandlerLabel);

      // Interlocked.Increment(ref this.callCountX)
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldflda, callCountFieldBuilder);
      ilGenerator.Emit (OpCodes.Call, s_interlockedIntIncrement);
      ilGenerator.Emit (OpCodes.Pop);

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
      return new GeneratedProxyMethodResult (nativeMockProxyCodeGeneratedMethod, handlerFieldBuilder, callCountFieldBuilder);
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
      var getMethodHandlerCountMethodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (getMethodHandlerCountInterfaceMethod);
      GenerateGetMethodHandlerCount (getMethodHandlerCountMethodBuilder, handlerFields.Length);

      // GetMethodHandlerCallCount (int methodHandle, Delegate handler)
      var getMethodHandlerCallCountInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.GetMethodHandlerCallCount))!;
      var getMethodHandlerCallCountMethodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (getMethodHandlerCallCountInterfaceMethod);
      GenerateGetMethod (getMethodHandlerCallCountMethodBuilder, callCountFields);

      // GetMethodHandler (int methodHandle)
      var getMethodHandlerInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.GetMethodHandler))!;
      var getMethodHandlerMethodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (getMethodHandlerInterfaceMethod);
      GenerateGetMethod (getMethodHandlerMethodBuilder, handlerFields);

      // SetHandler (int methodHandle, Delegate handler)
      var setHandlerInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.SetMethodHandler))!;
      var setHandlerMethodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (setHandlerInterfaceMethod);
      GenerateSetHandlerMethod (setHandlerMethodBuilder, handlerFields);

      // void SetUnderlyingImplementation (T underlyingImplementation);
      var setUnderlyingImplementationInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.SetUnderlyingImplementation))!;
      var setUnderlyingImplementationMethodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (setUnderlyingImplementationInterfaceMethod);
      GenerateSetUnderlyingImplementation (setUnderlyingImplementationMethodBuilder, interfaceType, underlyingImplementationField);

      // void Reset ();
      var resetInterfaceMethod = proxyControllerType.GetMethod (nameof(INativeMockProxyController<object>.Reset))!;
      var resetInterfaceMethodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (resetInterfaceMethod);
      GenerateReset (resetInterfaceMethodBuilder, underlyingImplementationField, handlerFields, callCountFields);
    }

    private void GenerateGetMethodHandlerCount (MethodBuilder methodBuilder, int methodCount)
    {
      var ilGenerator = methodBuilder.GetILGenerator();

      // return methodCount;
      ilGenerator.Emit (OpCodes.Ldc_I4, methodCount);
      ilGenerator.Emit (OpCodes.Ret);
    }

    private void GenerateGetMethod (
      MethodBuilder methodBuilder,
      FieldBuilder[] generatedMethodFields)
    {
      var ilGenerator = methodBuilder.GetILGenerator();

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
    }

    private void GenerateSetHandlerMethod (MethodBuilder methodBuilder, FieldBuilder[] generatedMethodFields)
    {
      var ilGenerator = methodBuilder.GetILGenerator();

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
    }

    private void GenerateSetUnderlyingImplementation (MethodBuilder methodBuilder, Type interfaceType, FieldBuilder underlyingImplementationField)
    {
      var ilGenerator = methodBuilder.GetILGenerator();

      // return this.underlyingImplementation;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldarg_1);
      ilGenerator.Emit (OpCodes.Stfld, underlyingImplementationField);
      ilGenerator.Emit (OpCodes.Ret);
    }

    private void GenerateReset (
      MethodBuilder methodBuilder,
      FieldBuilder underlyingImplementationField,
      FieldBuilder[] handlerFields,
      FieldBuilder[] callCountFields)
    {
      var ilGenerator = methodBuilder.GetILGenerator();

      // Clear the underlying implementation
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldnull);
      ilGenerator.Emit (OpCodes.Stfld, underlyingImplementationField);

      // Clear the call counts
      foreach (var handlerField in handlerFields)
      {
        ilGenerator.Emit (OpCodes.Ldarg_0);
        ilGenerator.Emit (OpCodes.Ldnull);
        ilGenerator.Emit (OpCodes.Stfld, handlerField);
      }

      // Clear the call counts
      foreach (var callCountField in callCountFields)
      {
        ilGenerator.Emit (OpCodes.Ldarg_0);
        ilGenerator.Emit (OpCodes.Ldc_I4_0);
        ilGenerator.Emit (OpCodes.Stfld, callCountField);
      }

      ilGenerator.Emit (OpCodes.Ret);
    }
  }
}
