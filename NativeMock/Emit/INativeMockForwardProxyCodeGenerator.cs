namespace NativeMock.Emit
{
  using System;
  using Representation;

  internal interface INativeMockForwardProxyCodeGenerator
  {
    Type CreateProxy (NativeMockInterfaceDescription nativeMockInterfaceDescription);
  }
}
