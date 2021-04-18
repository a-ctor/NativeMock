namespace NativeMock.Emit
{
  using System;
  using System.Runtime.InteropServices;
  using Representation;

  /// <inheritdoc />
  internal class NativeFunctionProxyFactory : INativeFunctionProxyFactory
  {
    private readonly IDelegateFactory _delegateFactory;
    private readonly INativeFunctionProxyCodeGenerator _nativeFunctionProxyCodeGenerator;

    public NativeFunctionProxyFactory (IDelegateFactory delegateFactory, INativeFunctionProxyCodeGenerator nativeFunctionProxyCodeGenerator)
    {
      if (delegateFactory == null)
        throw new ArgumentNullException (nameof(delegateFactory));
      if (nativeFunctionProxyCodeGenerator == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxyCodeGenerator));

      _delegateFactory = delegateFactory;
      _nativeFunctionProxyCodeGenerator = nativeFunctionProxyCodeGenerator;
    }

    public NativeFunctionProxy CreateNativeFunctionProxy (NativeMockInterfaceMethodDescription method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var proxyType = _delegateFactory.CreateDelegateType (method.PrototypeMethod);
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
