namespace NativeMock.Emit
{
  using System;
  using System.Runtime.InteropServices;
  using Representation;

  /// <inheritdoc />
  internal class NativeFunctionProxyFactory : INativeFunctionProxyFactory
  {
    private readonly IDelegateCodeGenerator _delegateCodeGenerator;
    private readonly INativeFunctionProxyCodeGenerator _nativeFunctionProxyCodeGenerator;

    public NativeFunctionProxyFactory (IDelegateCodeGenerator delegateCodeGenerator, INativeFunctionProxyCodeGenerator nativeFunctionProxyCodeGenerator)
    {
      if (delegateCodeGenerator == null)
        throw new ArgumentNullException (nameof(delegateCodeGenerator));
      if (nativeFunctionProxyCodeGenerator == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxyCodeGenerator));

      _delegateCodeGenerator = delegateCodeGenerator;
      _nativeFunctionProxyCodeGenerator = nativeFunctionProxyCodeGenerator;
    }

    public NativeFunctionProxy CreateNativeFunctionProxy (NativeMockInterfaceMethodDescription method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var proxyType = _delegateCodeGenerator.CreateDelegateType (method.StubTargetMethod);
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
