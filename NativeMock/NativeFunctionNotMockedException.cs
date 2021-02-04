namespace NativeMock
{
  using System;

  public class NativeFunctionNotMockedException : Exception
  {
    public NativeFunctionNotMockedException (NativeFunctionIdentifier functionName)
      : base ($"The native function '{functionName}' was called but not mock was found.")
    {
    }
  }
}
