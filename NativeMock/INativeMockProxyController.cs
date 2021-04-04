namespace NativeMock
{
  using System;

  public interface INativeMockProxyController
  {
    void SetMethodHandler (int methodHandle, Delegate handler);
  }
}
