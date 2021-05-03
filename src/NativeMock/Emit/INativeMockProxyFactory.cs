namespace NativeMock.Emit
{
  internal interface INativeMockProxyFactory
  {
    NativeMockProxy<T> CreateMockProxy<T>()
      where T : class;
  }
}
