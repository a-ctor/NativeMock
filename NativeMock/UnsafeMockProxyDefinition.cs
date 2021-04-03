namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Reflection;

  internal class UnsafeMockProxyDefinition<T>
    where T : class
  {
    public Type ProxyType { get; }

    private ImmutableDictionary<MethodInfo, int> MethodHandleLookup { get; }

    public UnsafeMockProxyDefinition (Type proxyType, ImmutableDictionary<MethodInfo, int> methodHandleLookup)
    {
      if (proxyType == null)
        throw new ArgumentNullException (nameof(proxyType));
      if (methodHandleLookup == null)
        throw new ArgumentNullException (nameof(methodHandleLookup));
      
      ProxyType = proxyType;
      MethodHandleLookup = methodHandleLookup;
    }

    public UnsafeMockProxy<T> CreateProxy()
    {
      var instance = Activator.CreateInstance (ProxyType)!;
      return new UnsafeMockProxy<T>((T) instance, (IUnsafeMockProxy) instance, MethodHandleLookup);
    }
  }
}
