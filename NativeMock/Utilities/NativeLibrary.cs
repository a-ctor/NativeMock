namespace NativeMock.Utilities
{
#if NET461
  using System;
  using System.ComponentModel;
  using System.Runtime.InteropServices;

  public static class NativeLibrary
  {
    private const long c_moduleNotFoundErrorCode = 0x7E;

    [DllImport ("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibraryW ([MarshalAs (UnmanagedType.LPWStr)] string fileName);

    public static IntPtr Load (string name)
    {
      var result = LoadLibraryW (name);
      if (result != IntPtr.Zero)
        return result;

      var lastWin32Error = Marshal.GetLastWin32Error();
      if (lastWin32Error == c_moduleNotFoundErrorCode)
        throw new DllNotFoundException();

      throw new Win32Exception (lastWin32Error);
    }
  }
#endif
}
