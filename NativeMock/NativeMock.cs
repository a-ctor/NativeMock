namespace NativeMock
{
  using System;

  public class NativeMock<T> : IDisposable
    where T : class
  {
    private readonly INativeMockSetupInternalRegistry _setupRegistry;

    private bool _disposed;

    public T Object { get; }

    public NativeMock (T implementation, NativeMockScope scope = NativeMockScope.Default)
      : this (NativeMockRegistry.GetSetupRegistryForScope (scope), implementation)
    {
    }

    internal NativeMock (INativeMockSetupInternalRegistry setupRegistry, T implementation)
    {
      if (setupRegistry == null)
        throw new ArgumentNullException (nameof(setupRegistry));
      if (implementation == null)
        throw new ArgumentNullException (nameof(implementation));
      if (!NativeMockRegistry.IsRegistered<T>())
        throw new InvalidOperationException ($"The specified type '{typeof(T)}' is not registered as a native mock interface. Use NativeMockRegistry.Register as early in the program as possible to register an interface.");
      if (!setupRegistry.TrySetup (implementation))
        throw new InvalidOperationException ("Cannot have two native mocks of the same interface in the same context at the same time.");

      _setupRegistry = setupRegistry;
      Object = implementation;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      if (_disposed)
        return;

      _setupRegistry.Reset<T>();
      _disposed = true;
    }
  }
}
