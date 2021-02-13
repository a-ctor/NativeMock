namespace NativeMock
{
  using System;

  /// <summary>
  /// Provides methods for identifying potential mock interface types in an assembly.
  /// </summary>
  public interface INativeMockInterfaceIdentifier
  {
    bool IsNativeMockInterfaceType (Type type);
  }
}
