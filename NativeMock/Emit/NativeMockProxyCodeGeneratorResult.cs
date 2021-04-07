namespace NativeMock.Emit
{
  using System;
  using System.Collections.Immutable;

  internal readonly struct NativeMockProxyCodeGeneratorResult
  {
    public readonly Type ProxyType;
    public readonly ImmutableArray<NativeMockProxyCodeGeneratedMethod> ProxiedMethods;

    public NativeMockProxyCodeGeneratorResult (Type proxyType, ImmutableArray<NativeMockProxyCodeGeneratedMethod> proxiedMethods)
    {
      ProxyType = proxyType;
      ProxiedMethods = proxiedMethods;
    }
  }
}
