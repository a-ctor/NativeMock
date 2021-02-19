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
    NativeMockInterfaceMethodDescription GetMockInterfaceDescription (
      string moduleName,
      MethodInfo method,
      Type? defaultDeclaringType,
      NativeMockBehavior defaultMockBehavior);
  }
}
