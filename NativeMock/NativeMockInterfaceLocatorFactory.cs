namespace NativeMock
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
    public INativeMockInterfaceLocator CreateMockInterfaceLocator (AutoRegisterSearchBehavior autoRegisterSearchBehavior)
    {
      return autoRegisterSearchBehavior switch
      {
        AutoRegisterSearchBehavior.TopLevelTypesOnly => new TopLevelNativeMockInterfaceLocator (_nativeMockInterfaceIdentifier),
        AutoRegisterSearchBehavior.IncludeNestedTypes => new NestedTypesNativeMockInterfaceLocator (_nativeMockInterfaceIdentifier),
        _ => throw new ArgumentOutOfRangeException (nameof(autoRegisterSearchBehavior), autoRegisterSearchBehavior, null)
      };
    }
  }
}
