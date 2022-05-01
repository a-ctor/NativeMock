namespace NativeMock.Utilities
{
  public static class PlatformUtility
  {
    public static void EnsurePlatformIsWindows()
    {
#if NET
      if (!System.OperatingSystem.IsWindows())
        throw new System.PlatformNotSupportedException ("NativeMock is only supported on the Windows platform.");
#endif
    }
  }
}
