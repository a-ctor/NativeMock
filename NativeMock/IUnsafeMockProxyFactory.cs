namespace NativeMock
{
  internal interface IUnsafeMockProxyFactory
  {
    UnsafeMockProxy<T> CreateMockProxy<T>()
      where T : class;
  }
}
