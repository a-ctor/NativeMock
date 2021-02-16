namespace NativeMock
{
  using System;
  using System.Collections.Immutable;

  /// <summary>
  /// Provides methods for retrieving <see cref="NativeMockModuleDescription" />s for a specific <see cref="Type" />.
  /// </summary>
  internal interface INativeMockModuleDescriptionProvider
  {
    ImmutableArray<NativeMockModuleDescription> GetMockModuleDescription (Type interfaceType);
  }
}
