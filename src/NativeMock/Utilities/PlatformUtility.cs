namespace NativeMock.Utilities
{
  using System;

  public static class PlatformUtility
  {
    public static void EnsurePlatformIsWindows()
    {
#if NET
      if (!OperatingSystem.IsWindows())
        throw new PlatformNotSupportedException ("NativeMock is only supported on the Windows platform.");
#endif
    }
  }
}
