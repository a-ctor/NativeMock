namespace NativeMock
{
  using System;

  internal interface INativeMockProxyCodeGenerator
  {
    NativeMockProxyCodeGeneratorResult CreateProxy (Type interfaceType);
  }
}
