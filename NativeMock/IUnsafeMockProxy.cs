namespace NativeMock
{
  using System;
  
  internal interface IUnsafeMockProxy
  {
    void SetHandler (int methodHandle, Delegate handler);
  }
}
