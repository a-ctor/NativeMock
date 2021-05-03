namespace NativeMock.Representation
{
  using System;

  /// <summary>
  /// Provides methods for identifying potential mock interface types in an assembly.
  /// </summary>
  internal interface INativeMockInterfaceIdentifier
  {
    bool IsNativeMockInterfaceType (Type type);
  }
}
