namespace NativeMock
{
  internal interface IDummyActionInterfaceMethodSelectorFactory
  {
    DummyActionInterfaceMethodSelector<T> CreateDummyActionInterfaceMethodSelector<T>()
      where T : class;
  }
}
