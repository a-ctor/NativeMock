namespace NativeMock.Emit
{
  using System;
  using System.Reflection;
  using System.Reflection.Emit;
  using System.Runtime.InteropServices;
  using Representation;

  internal class NativeMockForwardProxyCodeGenerator : INativeMockForwardProxyCodeGenerator
  {
    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
    private static readonly MethodInfo s_getTypeFromHandleMethod
      = ReflectionInfoUtility.SelectMethod (() => Type.GetTypeFromHandle (default));

    private static readonly MethodInfo s_getDelegateForFunctionPointerMethod
      // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
      = ReflectionInfoUtility.SelectMethod (() => Marshal.GetDelegateForFunctionPointer (default, default!));

    private readonly ModuleBuilder _moduleBuilder;
    private readonly IDelegateFactory _delegateFactory;
    private readonly MethodInfo _dllImportResolveMethod;

    public NativeMockForwardProxyCodeGenerator (ModuleBuilder moduleBuilder, IDelegateFactory delegateFactory, MethodInfo dllImportResolveMethod)
    {
      if (moduleBuilder == null)
        throw new ArgumentNullException (nameof(moduleBuilder));
      if (delegateFactory == null)
        throw new ArgumentNullException (nameof(delegateFactory));
      if (dllImportResolveMethod == null)
        throw new ArgumentNullException (nameof(dllImportResolveMethod));
      if (!dllImportResolveMethod.IsStatic)
        throw new ArgumentException ("Dll import resolve method must be static.", nameof(dllImportResolveMethod));
      if (dllImportResolveMethod.ReturnType != typeof(IntPtr))
        throw new ArgumentException ("Dll import resolve method must return a IntPtr.", nameof(dllImportResolveMethod));
      var parameters = dllImportResolveMethod.GetParameters();
      if (parameters.Length != 2 || parameters[0].ParameterType != typeof(string) || parameters[1].ParameterType != typeof(string))
        throw new ArgumentException ("Dll import resolve method must take two string parameters.", nameof(dllImportResolveMethod));

      _moduleBuilder = moduleBuilder;
      _delegateFactory = delegateFactory;
      _dllImportResolveMethod = dllImportResolveMethod;
    }

    /// <inheritdoc />
    public Type CreateProxy (NativeMockInterfaceDescription nativeMockInterfaceDescription)
    {
      if (nativeMockInterfaceDescription == null)
        throw new ArgumentNullException (nameof(nativeMockInterfaceDescription));

      var interfaceType = nativeMockInterfaceDescription.InterfaceType;
      var proxyTypeBuilder = _moduleBuilder.DefinePublicClass ($"{interfaceType.Name}_NativeMockForwardProxy_{Guid.NewGuid()}", typeof(object), interfaceType);

      // Implement the interface that we want to proxy
      var interfaceMethods = nativeMockInterfaceDescription.Methods;
      for (var i = 0; i < interfaceMethods.Length; i++)
        GenerateForwardMethod (proxyTypeBuilder, interfaceMethods[i], i);

      try
      {
        return proxyTypeBuilder.CreateType() ?? throw new InvalidOperationException ("Could not create the requested proxy type.");
      }
      catch (TypeLoadException ex)
      {
        throw new InvalidOperationException (
          $"Could not create the requested proxy type. Make sure that the specified interface is publicly accessible or internal with an [assembly: InternalsVisibleTo(\"{NativeMockRegistry.ProxyAssemblyName}\")] attribute.",
          ex);
      }
    }

    private void GenerateForwardMethod (TypeBuilder proxyTypeBuilder, NativeMockInterfaceMethodDescription methodDescription, int methodHandle)
    {
      var methodInfo = methodDescription.InterfaceMethod;

      var parameters = methodInfo.GetParameters();

      var delegateType = _delegateFactory.CreateDelegateType (methodInfo);
      var delegateInvokeMethod = delegateType.GetMethod ("Invoke")!;

      // Instance field containing the handler
      var forwardFieldBuilder = proxyTypeBuilder.DefinePrivateField (delegateType, $"field{methodHandle}");

      var methodBuilder = proxyTypeBuilder.DefineExplicitInterfaceMethodImplementation (methodInfo);
      var ilGenerator = methodBuilder.GetILGenerator();

      var callHandlerLabel = ilGenerator.DefineLabel();

      // if (this.fieldX != null) goto callHandler;
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, forwardFieldBuilder);
      ilGenerator.Emit (OpCodes.Brtrue_S, callHandlerLabel);

      // Helper.GetProcAddress("module", "function") 
      ilGenerator.Emit (OpCodes.Ldarg_0); // for stfld later
      ilGenerator.Emit (OpCodes.Ldstr, methodDescription.Name.ModuleName);
      ilGenerator.Emit (OpCodes.Ldstr, methodDescription.Name.FunctionName);
      ilGenerator.Emit (OpCodes.Call, _dllImportResolveMethod);

      // Marshal.GetDelegateForFunctionPointer(..., typeof(XDelegate));
      ilGenerator.Emit (OpCodes.Ldtoken, delegateType);
      ilGenerator.Emit (OpCodes.Call, s_getTypeFromHandleMethod);
      ilGenerator.Emit (OpCodes.Call, s_getDelegateForFunctionPointerMethod);

      // (XDelegate) ...
      ilGenerator.Emit (OpCodes.Castclass, delegateType);

      // this.fieldX = ...;
      ilGenerator.Emit (OpCodes.Stfld, forwardFieldBuilder);

      // callHandler:
      ilGenerator.MarkLabel (callHandlerLabel);

      // return this.fieldX(...args)
      ilGenerator.Emit (OpCodes.Ldarg_0);
      ilGenerator.Emit (OpCodes.Ldfld, forwardFieldBuilder);
      for (short i = 1; i <= parameters.Length; i++)
        ilGenerator.Emit (OpCodes.Ldarg, i);
      ilGenerator.Emit (OpCodes.Callvirt, delegateInvokeMethod);
      ilGenerator.Emit (OpCodes.Ret);
    }
  }
}
