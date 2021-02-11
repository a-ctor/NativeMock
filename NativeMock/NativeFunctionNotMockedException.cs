namespace NativeMock
{
  using System;

  /// <summary>
  /// The exception that is thrown when a hooked native method is called but no mock was registered for it.
  /// </summary>
  public class NativeFunctionNotMockedException : Exception
  {
    public NativeFunctionNotMockedException (string functionName)
      : base ($"The native function '{functionName}' was called but not mock was found.")
    {
    }
  }
}
