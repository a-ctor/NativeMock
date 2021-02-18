namespace NativeMock
{
  using System;
  using System.Runtime.InteropServices;

  /// <summary>
  /// Provides methods for crate <see cref="NativeFunctionProxy" />s for a specific native function.
  /// </summary>
  internal class NativeFunctionProxyFactory
  {
    private readonly DelegateGenerator _delegateGenerator;
    private readonly NativeFunctionProxyCodeGenerator _nativeFunctionProxyCodeGenerator;

    public NativeFunctionProxyFactory (DelegateGenerator delegateGenerator, NativeFunctionProxyCodeGenerator nativeFunctionProxyCodeGenerator)
    {
      if (delegateGenerator == null)
        throw new ArgumentNullException (nameof(delegateGenerator));
      if (nativeFunctionProxyCodeGenerator == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxyCodeGenerator));

      _delegateGenerator = delegateGenerator;
      _nativeFunctionProxyCodeGenerator = nativeFunctionProxyCodeGenerator;
    }

    public NativeFunctionProxy CreateNativeFunctionProxy (NativeMockInterfaceMethodDescription method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var proxyType = _delegateGenerator.CreateDelegateType (method.StubTargetMethod);
      var proxyMethod = _nativeFunctionProxyCodeGenerator.CreateProxyMethod (method, proxyType);
      var nativePtr = Marshal.GetFunctionPointerForDelegate (proxyMethod);

      return new NativeFunctionProxy (
        method.Name,
        proxyType,
        proxyMethod,
        nativePtr);
    }
  }
}
