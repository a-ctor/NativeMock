namespace NativeMock.Registration
{
  public interface INativeMockSetupRegistry
  {
    void Setup<T> (T implementation)
      where T : class;

    bool TrySetup<T> (T implementation)
      where T : class;

    void Reset<T>()
      where T : class;
  }
}
