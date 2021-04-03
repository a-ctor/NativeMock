namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;

  internal class NativeMockSetupRegistry
  {
    private readonly ConcurrentDictionary<Type, object> _setups = new();

    public bool TrySetup<T> (T implementation)
    {
      if (implementation == null)
        throw new ArgumentNullException();

      return _setups.TryAdd (typeof(T), implementation);
    }

    public T? GetSetup<T>()
      where T : class
    {
      return _setups.TryGetValue (typeof(T), out var result)
        ? (T) result
        : null;
    }

    public void Reset<T> (object implementation)
      where T : class
    {
      _setups.TryRemove (new KeyValuePair<Type, object> (typeof(T), implementation));
    }
  }
}
