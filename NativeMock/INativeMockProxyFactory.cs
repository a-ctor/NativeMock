namespace NativeMock
{
  internal interface INativeMockProxyFactory
  {
    NativeMockProxy<T> CreateMockProxy<T>()
      where T : class;
  }
}
