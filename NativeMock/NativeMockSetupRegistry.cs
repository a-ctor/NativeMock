namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;

  internal class NativeMockSetupRegistry : INativeMockSetupInternalRegistry
  {
    private readonly ConcurrentDictionary<Type, object> _setups = new();

    /// <inheritdoc />
    public void Setup<T> (T implementation)
      where T : class
    {
      if (implementation == null)
        throw new ArgumentNullException();

      if (!_setups.TryAdd (typeof(T), implementation))
        throw new InvalidOperationException ($"A setup for the mock interface '{typeof(T)}' was already registered.");
    }

    /// <inheritdoc />
    public bool TrySetup<T> (T implementation)
      where T : class
    {
      if (implementation == null)
        throw new ArgumentNullException();

      return _setups.TryAdd (typeof(T), implementation);
    }

    /// <inheritdoc />
    public T? GetSetup<T>()
      where T : class
    {
      return _setups.TryGetValue (typeof(T), out var result)
        ? (T) result
        : null;
    }

    /// <inheritdoc />
    public void Reset<T>()
      where T : class
    {
      _setups.TryRemove (typeof(T), out _);
    }
  }
}
