namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;

  internal class UnsafeMockProxyFactory : IUnsafeMockProxyFactory
  {
    private readonly UnsafeMockProxyGenerator _unsafeMockProxyGenerator;

    private readonly ConcurrentDictionary<Type, object> _mockProxyDefinitions = new();

    public UnsafeMockProxyFactory(UnsafeMockProxyGenerator unsafeMockProxyGenerator)
    {
      _unsafeMockProxyGenerator = unsafeMockProxyGenerator;
    }

    /// <inheritdoc />
    public UnsafeMockProxy<T> CreateMockProxy<T>()
      where T : class
    {
      var definition = _mockProxyDefinitions.GetOrAdd (typeof(T), _ => _unsafeMockProxyGenerator.CreateProxyDefinition<T>());
      return ((UnsafeMockProxyDefinition<T>) definition).CreateProxy();
    }
  }
}
