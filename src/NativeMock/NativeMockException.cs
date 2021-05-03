namespace NativeMock
{
  using System;

  public class NativeMockException : Exception
  {
    /// <inheritdoc />
    public NativeMockException (string message)
      : base (message)
    {
    }

    /// <inheritdoc />
    public NativeMockException (string message, Exception? innerException)
      : base (message, innerException)
    {
    }
  }
}
