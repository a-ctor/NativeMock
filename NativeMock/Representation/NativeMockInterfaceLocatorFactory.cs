namespace NativeMock.Representation
{
  using System;

  /// <inheritdoc />
  internal class NativeMockInterfaceLocatorFactory : INativeMockInterfaceLocatorFactory
  {
    private readonly INativeMockInterfaceIdentifier _nativeMockInterfaceIdentifier;

    public NativeMockInterfaceLocatorFactory (INativeMockInterfaceIdentifier nativeMockInterfaceIdentifier)
    {
      _nativeMockInterfaceIdentifier = nativeMockInterfaceIdentifier;
    }

    /// <inheritdoc />
    public INativeMockInterfaceLocator CreateMockInterfaceLocator (RegisterFromAssemblySearchBehavior registerFromAssemblySearchBehavior)
    {
      return (registerFromAssemblySearchBehavior & RegisterFromAssemblySearchBehavior.IncludeNestedTypes) != 0
        ? new NestedTypesNativeMockInterfaceLocator (_nativeMockInterfaceIdentifier)
        : new TopLevelNativeMockInterfaceLocator (_nativeMockInterfaceIdentifier);
    }
  }
}
