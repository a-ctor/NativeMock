namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;

  public class NativeMockCallbackRegistry
  {
    private readonly ConcurrentDictionary<NativeFunctionIdentifier, NativeMockCallback> _registeredCallbacks = new();

    public void Clear()
    {
      _registeredCallbacks.Clear();
    }

    public void Register (NativeFunctionIdentifier nativeFunctionIdentifier, NativeMockCallback callback)
    {
      if (nativeFunctionIdentifier.IsInvalid)
        throw new ArgumentNullException (nameof(nativeFunctionIdentifier));
      if (callback.IsInvalid)
        throw new ArgumentNullException (nameof(callback));

      if (!_registeredCallbacks.TryAdd (nativeFunctionIdentifier, callback))
        throw new InvalidOperationException ($"A callback with the identifier '{nativeFunctionIdentifier}' is already registered.");
    }

    public void Unregister (NativeFunctionIdentifier nativeFunctionIdentifier)
    {
      if (nativeFunctionIdentifier.IsInvalid)
        throw new ArgumentNullException (nameof(nativeFunctionIdentifier));

      _registeredCallbacks.TryRemove (nativeFunctionIdentifier, out _);
    }

    public object? Invoke (NativeFunctionIdentifier nativeFunctionIdentifier, object?[] args)
    {
      if (nativeFunctionIdentifier.IsInvalid)
        throw new ArgumentNullException (nameof(nativeFunctionIdentifier));
      if (args == null)
        throw new ArgumentNullException (nameof(args));

      if (!_registeredCallbacks.TryGetValue (nativeFunctionIdentifier, out var callback))
        throw new NativeFunctionNotMockedException (nativeFunctionIdentifier);

      return callback.Invoke (args);
    }
  }
}
