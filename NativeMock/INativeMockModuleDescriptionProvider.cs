namespace NativeMock
{
  using System;

  public interface INativeMockModuleDescriptionProvider
  {
    NativeMockModuleDescription? GetMockModuleDescription (Type interfaceType);
  }
}
