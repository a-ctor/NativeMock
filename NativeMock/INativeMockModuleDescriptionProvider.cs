namespace NativeMock
{
  using System;
  using System.Reflection;

  /// <summary>
  /// Provides methods for retrieving a <see cref="NativeMockModuleDescription" /> for a specific <see cref="Type" />.
  /// </summary>
  internal interface INativeMockModuleDescriptionProvider
  {
    NativeMockModuleDescription? GetMockModuleDescriptionForType (Type interfaceType);

    NativeMockModuleDescription? GetMockModuleDescriptionForMethod (MethodInfo method);
  }
}
