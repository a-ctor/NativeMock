namespace NativeMock.Emit
{
  internal interface INativeMockForwardProxyFactory
  {
    T CreateMockForwardProxy<T>()
      where T : class;
  }
}
