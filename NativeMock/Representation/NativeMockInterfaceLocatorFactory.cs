namespace NativeMock.Representation
{
  /// <inheritdoc />
  internal class NativeMockInterfaceLocatorFactory : INativeMockInterfaceLocatorFactory
  {
    private readonly INativeMockInterfaceIdentifierFactory _nativeMockInterfaceIdentifierFactory;

    public NativeMockInterfaceLocatorFactory (INativeMockInterfaceIdentifierFactory nativeMockInterfaceIdentifierFactory)
    {
      _nativeMockInterfaceIdentifierFactory = nativeMockInterfaceIdentifierFactory;
    }

    /// <inheritdoc />
    public INativeMockInterfaceLocator CreateMockInterfaceLocator (RegisterFromAssemblySearchBehavior registerFromAssemblySearchBehavior)
    {
      var nativeMockInterfaceIdentifier = _nativeMockInterfaceIdentifierFactory.CreateNativeMockIdentifier (registerFromAssemblySearchBehavior);
      INativeMockInterfaceLocator locator = (registerFromAssemblySearchBehavior & RegisterFromAssemblySearchBehavior.IncludeNestedTypes) != 0
        ? new NestedTypesNativeMockInterfaceLocator (nativeMockInterfaceIdentifier)
        : new TopLevelNativeMockInterfaceLocator (nativeMockInterfaceIdentifier);

      return locator;
    }
  }
}
