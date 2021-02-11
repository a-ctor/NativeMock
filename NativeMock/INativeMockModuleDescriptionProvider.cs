namespace NativeMock
{
  using System;

  /// <summary>
  /// Provides methods for retrieving a <see cref="NativeMockModuleDescription" /> for a specific <see cref="Type" />.
  /// </summary>
  internal interface INativeMockModuleDescriptionProvider
  {
    NativeMockModuleDescription? GetMockModuleDescription (Type interfaceType);
  }
}
