namespace NativeMock
{
  using System;

  public interface INativeMockInterfaceDescriptionProvider
  {
    NativeMockInterfaceDescription GetMockInterfaceDescription (Type interfaceType);
  }
}
