namespace NativeMock
{
  using System;
  using System.Runtime.InteropServices;

  /// <inheritdoc />
  internal class NativeFunctionProxyFactory : INativeFunctionProxyFactory
  {
    private readonly IDelegateGenerator _delegateGenerator;
    private readonly INativeFunctionProxyCodeGenerator _nativeFunctionProxyCodeGenerator;

    public NativeFunctionProxyFactory (IDelegateGenerator delegateGenerator, INativeFunctionProxyCodeGenerator nativeFunctionProxyCodeGenerator)
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
