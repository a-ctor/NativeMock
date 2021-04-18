namespace NativeMock.Emit
{
  using System;

  internal interface INativeMockProxyController<in T>
    where T : class
  {
    int GetMethodCount();

    int GetMethodHandlerCallCount (int methodHandle);

    void SetUnderlyingImplementation (T? underlyingImplementation);

    Delegate? GetMethodHandler (int methodHandle);

    void SetMethodHandler (int methodHandle, Delegate handler);

    void Reset();
  }
}
