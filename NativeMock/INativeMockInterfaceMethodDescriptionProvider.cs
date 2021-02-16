namespace NativeMock
{
  using System;
  using System.Reflection;

  /// <summary>
  /// Provides methods for retrieving a <see cref="NativeMockInterfaceMethodDescription" /> for a specific
  /// <see cref="MethodInfo" />.
  /// </summary>
  internal interface INativeMockInterfaceMethodDescriptionProvider
  {
    NativeMockInterfaceMethodDescription GetMockInterfaceDescription (MethodInfo method, Type? defaultDeclaringType, string? defaultModuleName);
  }
}
