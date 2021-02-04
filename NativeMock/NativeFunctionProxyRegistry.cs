namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;
  using System.Threading;

  internal class NativeFunctionProxyRegistry
  {
    private class Basket
    {
      private NativeFunctionProxy? _globalProxy;

      public NativeFunctionIdentifier Name { get; }

      public NativeFunctionProxy? GlobalProxy => _globalProxy;

      public Basket (NativeFunctionIdentifier name)
      {
        Name = name;
      }

      public void RegisterGlobalProxy (NativeFunctionProxy nativeFunctionProxy)
      {
        if (Interlocked.CompareExchange (ref _globalProxy, nativeFunctionProxy, null) != null)
          throw new InvalidOperationException ($"A global proxy for the function '{Name}' is already registered.");
      }
    }

    private readonly object _writeLock = new();
    private readonly Func<NativeFunctionIdentifier, Basket> _basketFactory;

    private readonly ConcurrentDictionary<NativeFunctionIdentifier, Basket> _baskets = new();

    public NativeFunctionProxyRegistry()
    {
      _basketFactory = functionName => new Basket (functionName);
    }

    public bool IsRegistered (string functionName) => _baskets.TryGetValue (new NativeFunctionIdentifier (functionName), out var basket) && basket.GlobalProxy != null;

    public void Register (NativeFunctionProxy nativeFunctionProxy)
    {
      if (nativeFunctionProxy == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxy));

      lock (_writeLock)
      {
        var basket = _baskets.GetOrAdd (nativeFunctionProxy.Name, _basketFactory);
        basket.RegisterGlobalProxy (nativeFunctionProxy);
      }
    }

    public NativeFunctionProxy? Resolve (string functionName)
    {
      if (functionName == null)
        throw new ArgumentNullException (functionName);

      return _baskets.TryGetValue (new NativeFunctionIdentifier (functionName), out var basket)
        ? basket.GlobalProxy
        : null;
    }
  }
}
