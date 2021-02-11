namespace NativeMock
{
  using System;

  internal interface INativeMockInterfaceDescriptionProvider
  {
    NativeMockInterfaceDescription GetMockInterfaceDescription (Type interfaceType);
  }
}
