namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;

  /// <summary>
  /// Provides a thread-safe registry for <see cref="NativeMockCallback" />s instances that can be resolved using their
  /// <see cref="NativeFunctionIdentifier" />.
  /// </summary>
  internal class NativeMockCallbackRegistry
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

    /// <exception cref="NativeFunctionNotMockedException">
    /// No mock was registerd for the function specified by <paramref name="nativeFunctionIdentifier" />.
    /// </exception>
    public bool TryInvoke (NativeFunctionIdentifier nativeFunctionIdentifier, object?[] args, out object? result)
    {
      if (nativeFunctionIdentifier.IsInvalid)
        throw new ArgumentNullException (nameof(nativeFunctionIdentifier));
      if (args == null)
        throw new ArgumentNullException (nameof(args));

      if (!_registeredCallbacks.TryGetValue (nativeFunctionIdentifier, out var callback))
      {
        result = default;
        return false;
      }

      result = callback.Invoke (args);
      return true;
    }
  }
}
