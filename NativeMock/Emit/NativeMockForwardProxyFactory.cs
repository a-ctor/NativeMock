namespace NativeMock.Emit
{
  using System;
  using System.Collections.Concurrent;

  internal class NativeMockForwardProxyFactory : INativeMockForwardProxyFactory
  {
    private readonly INativeMockForwardProxyCodeGenerator _nativeMockForwardProxyCodeGenerator;
    private readonly INativeMockInterfaceDescriptionLookup _nativeMockInterfaceDescriptionLookup;

    private readonly ConcurrentDictionary<Type, object> _forwardProxies = new();

    public NativeMockForwardProxyFactory (INativeMockForwardProxyCodeGenerator nativeMockForwardProxyCodeGenerator, INativeMockInterfaceDescriptionLookup nativeMockInterfaceDescriptionLookup)
    {
      if (nativeMockForwardProxyCodeGenerator == null)
        throw new ArgumentNullException (nameof(nativeMockForwardProxyCodeGenerator));
      if (nativeMockInterfaceDescriptionLookup == null)
        throw new ArgumentNullException (nameof(nativeMockInterfaceDescriptionLookup));

      _nativeMockForwardProxyCodeGenerator = nativeMockForwardProxyCodeGenerator;
      _nativeMockInterfaceDescriptionLookup = nativeMockInterfaceDescriptionLookup;
    }

    /// <inheritdoc />
    public T CreateMockForwardProxy<T>()
      where T : class
    {
      if (_forwardProxies.TryGetValue (typeof(T), out var cachedForwardProxy))
        return (T) cachedForwardProxy;

      var description = _nativeMockInterfaceDescriptionLookup.GetMockInterfaceDescription<T>();
      if (description == null)
        throw new InvalidOperationException ($"Cannot find an interface description for the type '{typeof(T)}'.");

      var forwardProxyType = _nativeMockForwardProxyCodeGenerator.CreateProxy (description);
      var forwardProxyInstance = Activator.CreateInstance (forwardProxyType)!;

      _forwardProxies.TryAdd (typeof(T), forwardProxyInstance);
      return (T) forwardProxyInstance;
    }
  }
}
