namespace NativeMock
{
  public interface INativeMockSetupInternalRegistry : INativeMockSetupRegistry
  {
    public T? GetSetup<T>()
      where T : class;
  }
}