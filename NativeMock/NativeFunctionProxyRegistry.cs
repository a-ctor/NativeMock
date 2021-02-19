namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;

  /// <summary>
  /// Provides a thread-safe registry for <see cref="NativeMockRepository" /> instances that can be resolved using their
  /// <see cref="NativeFunctionIdentifier" />.
  /// </summary>
  internal class NativeFunctionProxyRegistry
  {
    private readonly ConcurrentDictionary<NativeFunctionIdentifier, NativeFunctionProxy> _proxies = new();

    public NativeFunctionProxyRegistry()
    {
    }

    public void Register (NativeFunctionProxy nativeFunctionProxy)
    {
      if (nativeFunctionProxy == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxy));

      if (!_proxies.TryAdd (nativeFunctionProxy.Name, nativeFunctionProxy))
        throw new InvalidOperationException ($"A native function proxy for '{nativeFunctionProxy.Name}' is already registered.");
    }

    public NativeFunctionProxy? Resolve (NativeFunctionIdentifier nativeFunctionIdentifier)
    {
      if (nativeFunctionIdentifier.IsInvalid)
        throw new ArgumentNullException (nameof(nativeFunctionIdentifier));

      return _proxies.TryGetValue (nativeFunctionIdentifier, out var proxy)
        ? proxy
        : null;
    }
  }
}
