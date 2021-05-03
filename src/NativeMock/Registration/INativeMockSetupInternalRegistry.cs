namespace NativeMock.Registration
{
  internal interface INativeMockSetupInternalRegistry : INativeMockSetupRegistry
  {
    public T? GetSetup<T>()
      where T : class;
  }
}
