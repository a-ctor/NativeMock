namespace NativeMock
{
  using System;

  public class NativeFunctionNotMockedException : Exception
  {
    public NativeFunctionNotMockedException (string functionName)
      : base ($"The native function '{functionName}' was called but not mock was found.")
    {
    }
  }
}
