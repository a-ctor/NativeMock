namespace NativeMock.Representation
{
  internal interface INativeMockInterfaceIdentifierFactory
  {
    INativeMockInterfaceIdentifier CreateNativeMockIdentifier (RegisterFromAssemblySearchBehavior registerFromAssemblySearchBehavior);
  }
}
