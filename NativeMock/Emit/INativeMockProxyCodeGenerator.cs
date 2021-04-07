namespace NativeMock.Emit
{
  using System;

  internal interface INativeMockProxyCodeGenerator
  {
    NativeMockProxyCodeGeneratorResult CreateProxy (Type interfaceType);
  }
}
