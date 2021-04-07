namespace NativeMock.Emit
{
  using System;

  public interface INativeMockProxyController<in T>
    where T : class
  {
    void SetUnderlyingImplementation (T? underlyingImplementation);

    void SetMethodHandler (int methodHandle, Delegate handler);
  }
}
