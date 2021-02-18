namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;

  internal class NativeMockSetupRegistry
  {
    private readonly ConcurrentDictionary<Type, object> _setups = new();

    public void Add<T> (T implementation)
      where T : class
    {
      if (implementation == null)
        throw new ArgumentNullException();

      if (!_setups.TryAdd (typeof(T), implementation))
        throw new InvalidOperationException ($"A setup for the mock interface '{typeof(T)}' was already registered.");
    }

    public T? Get<T>()
      where T : class
    {
      return _setups.TryGetValue (typeof(T), out var result)
        ? (T) result
        : null;
    }

    public void Clear()
    {
      _setups.Clear();
    }
  }
}
