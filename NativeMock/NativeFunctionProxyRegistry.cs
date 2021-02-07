namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;
  using System.Threading;

  internal class NativeFunctionProxyRegistry
  {
    private class Basket
    {
      private readonly ConcurrentDictionary<string, NativeFunctionProxy> _moduleProxies = new (StringComparer.OrdinalIgnoreCase);

      private NativeFunctionProxy? _globalProxy;

      public Basket()
      {
      }

      public NativeFunctionProxy? Resolve (NativeFunctionIdentifier nativeFunctionIdentifier)
      {
        if (nativeFunctionIdentifier.ModuleName != null && _moduleProxies.TryGetValue (nativeFunctionIdentifier.ModuleName, out var proxy))
          return proxy;

        return _globalProxy;
      }

      public void Register (NativeFunctionProxy nativeFunctionProxy)
      {
        var nativeFunctionIdentifier = nativeFunctionProxy.Name;
        if (nativeFunctionIdentifier.ModuleName == null)
        {
          if (Interlocked.CompareExchange (ref _globalProxy, nativeFunctionProxy, null) != null)
            throw new InvalidOperationException ($"A global proxy for '{nativeFunctionIdentifier}' is already registered.");
        }
        else
        {
          if (!_moduleProxies.TryAdd (nativeFunctionIdentifier.ModuleName, nativeFunctionProxy))
            throw new InvalidOperationException ($"A module proxy for '{nativeFunctionIdentifier}' is already registered.");
        }
      }
    }

    private readonly object _writeLock = new();
    private readonly Func<string, Basket> _basketFactory;

    private readonly ConcurrentDictionary<string, Basket> _baskets = new (StringComparer.OrdinalIgnoreCase);

    public NativeFunctionProxyRegistry()
    {
      _basketFactory = _ => new Basket();
    }

    public void Register (NativeFunctionProxy nativeFunctionProxy)
    {
      if (nativeFunctionProxy == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxy));

      lock (_writeLock)
      {
        var basket = _baskets.GetOrAdd (nativeFunctionProxy.Name.FunctionName, _basketFactory);
        basket.Register (nativeFunctionProxy);
      }
    }

    public NativeFunctionProxy? Resolve (NativeFunctionIdentifier nativeFunctionIdentifier)
    {
      if (nativeFunctionIdentifier.IsInvalid)
        throw new ArgumentNullException (nameof(nativeFunctionIdentifier));

      return _baskets.TryGetValue (nativeFunctionIdentifier.FunctionName, out var basket)
        ? basket.Resolve (nativeFunctionIdentifier)
        : null;
    }
  }
}
