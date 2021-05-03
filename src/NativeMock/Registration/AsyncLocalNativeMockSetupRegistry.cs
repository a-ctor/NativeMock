namespace NativeMock.Registration
{
  using System;
  using System.Threading;

  internal class AsyncLocalNativeMockSetupRegistry : INativeMockSetupInternalRegistry
  {
    private static readonly AsyncLocal<INativeMockSetupInternalRegistry> s_nativeMockSetupRegistry = new();

    private readonly INativeMockSetupInternalRegistryFactory _nativeMockSetupInternalRegistryFactory;

    public AsyncLocalNativeMockSetupRegistry (INativeMockSetupInternalRegistryFactory nativeMockSetupInternalRegistryFactory)
    {
      if (nativeMockSetupInternalRegistryFactory == null)
        throw new ArgumentNullException (nameof(nativeMockSetupInternalRegistryFactory));

      _nativeMockSetupInternalRegistryFactory = nativeMockSetupInternalRegistryFactory;
    }

    /// <inheritdoc />
    public void Setup<T> (T implementation)
      where T : class
    {
      var currentNativeMockSetupRegistry = GetOrCreateCurrentNativeMockSetupRegistry();
      currentNativeMockSetupRegistry.Setup (implementation);
    }

    /// <inheritdoc />
    public bool TrySetup<T> (T implementation)
      where T : class
    {
      var currentNativeMockSetupRegistry = GetOrCreateCurrentNativeMockSetupRegistry();
      return currentNativeMockSetupRegistry.TrySetup (implementation);
    }

    /// <inheritdoc />
    public void Reset<T>()
      where T : class
    {
      var currentNativeMockSetupRegistry = GetOrCreateCurrentNativeMockSetupRegistry();
      currentNativeMockSetupRegistry.Reset<T>();
    }

    /// <inheritdoc />
    public T? GetSetup<T>()
      where T : class
    {
      return s_nativeMockSetupRegistry.Value?.GetSetup<T>();
    }

    private INativeMockSetupInternalRegistry GetOrCreateCurrentNativeMockSetupRegistry() => s_nativeMockSetupRegistry.Value ??= _nativeMockSetupInternalRegistryFactory.CreateMockSetupRegistry();
  }
}
