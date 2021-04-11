namespace NativeMock.Emit
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;
  using System.Runtime.InteropServices;
  using Representation;

  internal class NativeMockForwardProxyCodeGenerator : INativeMockForwardProxyCodeGenerator
  {
    private const TypeAttributes c_classTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;

    private const MethodAttributes c_implicitMethodImplementationAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

    private const FieldAttributes c_instanceFieldAttributes = FieldAttributes.Private;

    private static readonly MethodInfo s_getTypeFromHandleMethod = typeof(Type).GetMethod ("GetTypeFromHandle")!;

    private static readonly MethodInfo s_getDelegateForFunctionPointerMethod = typeof(Marshal).GetMethod (nameof(Marshal.GetDelegateForFunctionPointer), new[] {typeof(IntPtr), typeof(Type)})!;

    private readonly ModuleBuilder _moduleBuilder;
    private readonly IDelegateCodeGenerator _delegateCodeGenerator;
    private readonly MethodInfo _dllImportResolveMethod;

    public NativeMockForwardProxyCodeGenerator (ModuleBuilder moduleBuilder, IDelegateCodeGenerator delegateCodeGenerator, MethodInfo dllImportResolveMethod)
    {
      if (moduleBuilder == null)
        throw new ArgumentNullException (nameof(moduleBuilder));
      if (delegateCodeGenerator == null)
        throw new ArgumentNullException (nameof(delegateCodeGenerator));
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
      _delegateCodeGenerator = delegateCodeGenerator;
      _dllImportResolveMethod = dllImportResolveMethod;
    }

    /// <inheritdoc />
    public Type CreateProxy (NativeMockInterfaceDescription nativeMockInterfaceDescription)
    {
      if (nativeMockInterfaceDescription == null)
        throw new ArgumentNullException (nameof(nativeMockInterfaceDescription));

      var interfaceType = nativeMockInterfaceDescription.InterfaceType;
      var proxyTypeBuilder = _moduleBuilder.DefineType ($"{interfaceType.Name}_NativeMockForwardProxy_{Guid.NewGuid()}", c_classTypeAttributes, typeof(object), new[] {interfaceType});

      // Implement the interface that we want to proxy
      var interfaceMethods = nativeMockInterfaceDescription.Methods;
      for (var i = 0; i < interfaceMethods.Length; i++)
        GenerateForwardMethod (proxyTypeBuilder, interfaceMethods[i], i);

      // todo: do accessibility check and throw if inaccessible
      var proxyType = proxyTypeBuilder.CreateType();
      if (proxyType == null)
        throw new InvalidOperationException ("Could not create the requested proxy type.");

      return proxyType;
    }

    private void GenerateForwardMethod (TypeBuilder proxyTypeBuilder, NativeMockInterfaceMethodDescription methodDescription, int methodHandle)
    {
      var methodInfo = methodDescription.InterfaceMethod;

      var returnType = methodInfo.ReturnType;
      var parameters = methodInfo.GetParameters().Select (e => e.ParameterType).ToArray();

      var delegateType = _delegateCodeGenerator.CreateDelegateType (methodInfo);
      var delegateInvokeMethod = delegateType.GetMethod ("Invoke")!;

      // Instance field containing the handler
      var forwardFieldBuilder = proxyTypeBuilder.DefineField ($"field{methodHandle}", delegateType, c_instanceFieldAttributes);

      var methodBuilder = proxyTypeBuilder.DefineMethod (methodInfo.Name, c_implicitMethodImplementationAttributes, returnType, parameters);
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
