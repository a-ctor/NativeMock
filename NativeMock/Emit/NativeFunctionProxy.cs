namespace NativeMock.Emit
{
  using System;

  /// <summary>
  /// Represents a proxy method that was generated as a replacement for a native method.
  /// </summary>
  internal class NativeFunctionProxy
  {
    public NativeFunctionIdentifier Name { get; }

    public Type ProxyType { get; }

    public Delegate Proxy { get; }

    public IntPtr NativePtr { get; }

    public NativeFunctionProxy (
      NativeFunctionIdentifier name,
      Type proxyType,
      Delegate proxy,
      IntPtr nativePtr)
    {
      if (name.IsInvalid)
        throw new ArgumentNullException (nameof(name));
      if (proxyType == null)
        throw new ArgumentNullException (nameof(proxyType));
      if (proxy == null)
        throw new ArgumentNullException (nameof(proxy));
      if (!proxyType.IsAssignableTo (typeof(Delegate)))
        throw new ArgumentException ("Must be a delegate type.", nameof(proxyType));
      if (!proxy.GetType().IsAssignableTo (proxyType))
        throw new ArgumentException ("Must be assignable to the specified delegate type.");

      Name = name;
      ProxyType = proxyType;
      Proxy = proxy;
      NativePtr = nativePtr;
    }
  }
}
