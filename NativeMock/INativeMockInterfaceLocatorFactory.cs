namespace NativeMock
{
  /// <summary>
  /// Provides methods for creating <see cref="INativeMockInterfaceLocator" />s for a specified
  /// <see cref="AutoRegisterSearchBehavior" />.
  /// </summary>
  internal interface INativeMockInterfaceLocatorFactory
  {
    INativeMockInterfaceLocator CreateMockInterfaceLocator (AutoRegisterSearchBehavior autoRegisterSearchBehavior);
  }
}
