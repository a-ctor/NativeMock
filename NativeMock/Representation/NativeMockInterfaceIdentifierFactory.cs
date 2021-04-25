namespace NativeMock.Representation
{
  internal class NativeMockInterfaceIdentifierFactory : INativeMockInterfaceIdentifierFactory
  {
    /// <inheritdoc />
    public INativeMockInterfaceIdentifier CreateNativeMockIdentifier (RegisterFromAssemblySearchBehavior registerFromAssemblySearchBehavior)
    {
      INativeMockInterfaceIdentifier? nativeMockInterfaceIdentifier = new NativeMockInterfaceIdentifier();

      if ((registerFromAssemblySearchBehavior & RegisterFromAssemblySearchBehavior.IncludePrivateTypes) == 0)
        nativeMockInterfaceIdentifier = new PublicTypesOnlyNativeMockInterfaceIdentifierDecorator (nativeMockInterfaceIdentifier);

      return nativeMockInterfaceIdentifier;
    }
  }
}
