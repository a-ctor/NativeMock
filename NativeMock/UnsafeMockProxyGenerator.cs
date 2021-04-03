namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;

  internal class UnsafeMockProxyGenerator
  {
    private readonly struct GeneratedMethodInfo
    {
      public readonly MethodInfo MethodInfo;
      public readonly int Handle;
      public readonly FieldBuilder InstanceFieldBuilder;

      public GeneratedMethodInfo (MethodInfo methodInfo, int handle, FieldBuilder instanceFieldBuilder)
      {
        if (methodInfo == null)
          throw new ArgumentNullException (nameof(methodInfo));
        if (instanceFieldBuilder == null)
          throw new ArgumentNullException (nameof(instanceFieldBuilder));

        MethodInfo = methodInfo;
        Handle = handle;
        InstanceFieldBuilder = instanceFieldBuilder;
      }
    }

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

    private static readonly MethodInfo s_proxySetHandlerMethod = typeof(IUnsafeMockProxy).GetMethod (nameof(IUnsafeMockProxy.SetHandler))!;

    private readonly ModuleBuilder _moduleBuilder;
    private readonly DelegateGenerator _delegateGenerator;

    public UnsafeMockProxyGenerator (AssemblyName assemblyName, string moduleName)
    {
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
      _moduleBuilder = assemblyBuilder.DefineDynamicModule (moduleName);
      _delegateGenerator = new DelegateGenerator (_moduleBuilder);
    }

    public UnsafeMockProxyDefinition<T> CreateProxyDefinition<T>()
      where T : class
    {
      var interfaceType = typeof(T);
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type must be an interface.");

      var proxyTypeBuilder = _moduleBuilder.DefineType ($"{interfaceType.Name}_NativeFunctionProxyDelegate_{Guid.NewGuid()}", c_classTypeAttributes, typeof(object), new[] {interfaceType, typeof(IUnsafeMockProxy)});

      // Implement the interface that we want to proxy
      var generatedProxyMethods = interfaceType.GetMethods()
        .Select ((e, i) => GenerateProxyMethod (proxyTypeBuilder, e, i + 1))
        .ToArray();

      var generatedProxyMethodLookup = generatedProxyMethods.ToImmutableDictionary (e => e.MethodInfo, e => e.Handle);

      // Implement the proxy interface
      GenerateProxyMetaMethods (proxyTypeBuilder, generatedProxyMethods);
      
      Type? proxyType;
      //try
      //{
      proxyType = proxyTypeBuilder.CreateType();
      //}
      //catch (TypeLoadException ex) when (!interfaceType.IsPublic)
      //{
      //  throw new ArgumentException ($"Can not create proxy for type '{interfaceType}' because it is not accessible. Make it public, or internal and and mark your assembly with [assembly: InternalsVisibleTo(\"{_moduleBuilder.FullyQualifiedName}\")]", nameof(interfaceType), ex);
      //}

      if (proxyType == null)
        throw new InvalidOperationException ("Could not create the requested proxy type.");

      return new UnsafeMockProxyDefinition<T> (proxyType, generatedProxyMethodLookup);
    }

    private GeneratedMethodInfo GenerateProxyMethod (TypeBuilder proxyTypeBuilder, MethodInfo methodInfo, int methodHandle)
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

      return new GeneratedMethodInfo (methodInfo, methodHandle, handlerFieldBuilder);
    }
    
    private void GenerateProxyMetaMethods (TypeBuilder proxyTypeBuilder, GeneratedMethodInfo[] generatedMethodInfos)
    {
      // SetHandler (int methodHandle, Delegate handler)
      var setHandlerMethod = proxyTypeBuilder.DefineMethod (
        $"{nameof(IUnsafeMockProxy)}.{nameof(IUnsafeMockProxy.SetHandler)}",
        c_explicitMethodImplementationAttributes,
        typeof(void),
        new[] {typeof(int), typeof(Delegate)});

      var ilGenerator = setHandlerMethod.GetILGenerator();

      Label[] switchLabels = new Label[generatedMethodInfos.Length];
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
        var generatedMethodInfo = generatedMethodInfos[i];
        var field = generatedMethodInfo.InstanceFieldBuilder;
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
