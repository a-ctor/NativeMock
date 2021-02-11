namespace NativeMock
{
  using System;

  internal interface INativeMockModuleDescriptionProvider
  {
    NativeMockModuleDescription? GetMockModuleDescription (Type interfaceType);
  }
}
