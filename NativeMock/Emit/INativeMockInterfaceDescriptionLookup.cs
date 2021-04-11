namespace NativeMock.Emit
{
  using Representation;

  internal interface INativeMockInterfaceDescriptionLookup
  {
    NativeMockInterfaceDescription? GetMockInterfaceDescription<T>()
      where T : class;
  }
}
