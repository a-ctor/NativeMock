namespace NativeMock.Representation
{
  using System;

  /// <summary>
  /// Provides methods for retrieving a <see cref="NativeMockInterfaceMethodDescription" /> for a specific
  /// <see cref="Type" />.
  /// </summary>
  internal interface INativeMockInterfaceDescriptionProvider
  {
    NativeMockInterfaceDescription GetMockInterfaceDescription (Type interfaceType);
  }
}
