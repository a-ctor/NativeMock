namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Immutable;

  internal class NativeMockProxyFactory : INativeMockProxyFactory
  {
    private readonly INativeMockProxyCodeGenerator _nativeMockProxyCodeGenerator;

    private readonly ConcurrentDictionary<Type, object> _proxyDefinitions = new();

    public NativeMockProxyFactory (INativeMockProxyCodeGenerator nativeMockProxyCodeGenerator)
    {
      if (nativeMockProxyCodeGenerator == null)
        throw new ArgumentNullException (nameof(nativeMockProxyCodeGenerator));

      _nativeMockProxyCodeGenerator = nativeMockProxyCodeGenerator;
    }

    /// <inheritdoc />
    public NativeMockProxy<T> CreateMockProxy<T>()
      where T : class
    {
      if (!typeof(T).IsInterface)
        throw new ArgumentException ("The specified type must be an interface.");

      var nativeMockProxyDefinition = (NativeMockProxyDefinition<T>) _proxyDefinitions.GetOrAdd (typeof(T), GetNativeMockProxyDefinition<T>());
      return nativeMockProxyDefinition.CreateProxy();
    }

    private NativeMockProxyDefinition<T> GetNativeMockProxyDefinition<T>()
      where T : class
    {
      var proxyCodeGeneratorResult = _nativeMockProxyCodeGenerator.CreateProxy (typeof(T));
      var methodHandleLookup = proxyCodeGeneratorResult.ProxiedMethods.ToImmutableDictionary (e => e.MethodInfo, e => e.MethodHandle);
      return new NativeMockProxyDefinition<T> (proxyCodeGeneratorResult.ProxyType, methodHandleLookup);
    }
  }
}
