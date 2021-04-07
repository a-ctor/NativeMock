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
      return registerFromAssemblySearchBehavior switch
      {
        RegisterFromAssemblySearchBehavior.TopLevelTypesOnly => new TopLevelNativeMockInterfaceLocator (_nativeMockInterfaceIdentifier),
        RegisterFromAssemblySearchBehavior.IncludeNestedTypes => new NestedTypesNativeMockInterfaceLocator (_nativeMockInterfaceIdentifier),
        _ => throw new ArgumentOutOfRangeException (nameof(registerFromAssemblySearchBehavior), registerFromAssemblySearchBehavior, null)
      };
    }
  }
}
