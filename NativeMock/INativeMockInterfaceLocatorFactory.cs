namespace NativeMock
{
  /// <summary>
  /// Provides methods for creating <see cref="INativeMockInterfaceLocator" />s for a specified
  /// <see cref="RegisterFromAssemblySearchBehavior" />.
  /// </summary>
  internal interface INativeMockInterfaceLocatorFactory
  {
    INativeMockInterfaceLocator CreateMockInterfaceLocator (RegisterFromAssemblySearchBehavior registerFromAssemblySearchBehavior);
  }
}
