namespace NativeMock
{
  using System;
  using System.Runtime.InteropServices;

  /// <summary>
  /// Provides methods for crate <see cref="NativeFunctionProxy" />s for a specific native function.
  /// </summary>
  internal class NativeFunctionProxyFactory
  {
    private readonly NativeFunctionProxyCodeGenerator _nativeFunctionProxyCodeGenerator;
    private readonly NativeFunctionHook _proxyTarget;

    public NativeFunctionProxyFactory (NativeFunctionProxyCodeGenerator nativeFunctionProxyCodeGenerator, NativeFunctionHook proxyTarget)
    {
      if (proxyTarget == null)
        throw new ArgumentNullException (nameof(proxyTarget));
      if (!proxyTarget.Method.IsStatic)
        throw new ArgumentException ("The specified native call proxy must have a static target.");

      _nativeFunctionProxyCodeGenerator = nativeFunctionProxyCodeGenerator;
      _proxyTarget = proxyTarget;
    }

    public NativeFunctionProxy CreateNativeFunctionProxy (NativeFunctionIdentifier name, Type nativeFunctionDelegateType)
    {
      if (name.IsInvalid)
        throw new ArgumentNullException (nameof(name));
      if (nativeFunctionDelegateType == null)
        throw new ArgumentNullException (nameof(nativeFunctionDelegateType));
      if (!nativeFunctionDelegateType.IsSubclassOf (typeof(Delegate)))
        throw new ArgumentException ("The specified native function type must be a delegate", nameof(nativeFunctionDelegateType));

      var nativeFunctionDelegate = _nativeFunctionProxyCodeGenerator.CreateProxyMethod (name, nativeFunctionDelegateType, _proxyTarget);
      var defaultStub = _nativeFunctionProxyCodeGenerator.CreateDefaultStub (name, nativeFunctionDelegateType);
      var nativePtr = Marshal.GetFunctionPointerForDelegate (nativeFunctionDelegate);

      return new NativeFunctionProxy (
        name,
        nativeFunctionDelegateType,
        nativeFunctionDelegate,
        nativePtr,
        defaultStub);
    }
  }
}
