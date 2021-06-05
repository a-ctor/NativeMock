namespace NativeMock.Hooking
{
  using System;
  using System.Runtime.InteropServices;
  using System.Threading;

  public class VirtualMemoryProtectionScope : IDisposable
  {
    private const uint c_pageExecuteReadWrite = 0x40;

    [DllImport ("kernel32.dll")]
    private static extern bool VirtualProtect (IntPtr lpAddress, nint dwSize, uint flNewProtect, out uint lpflOldProtect);

    public static VirtualMemoryProtectionScope SetReadWrite (IntPtr ptr, nint size)
    {
      VirtualProtect (ptr, size, c_pageExecuteReadWrite, out var oldProtect);
      return new VirtualMemoryProtectionScope (ptr, size, oldProtect);
    }

    private readonly IntPtr _ptr;
    private readonly nint _size;
    private readonly uint _oldProtect;

    private int _disposed;

    public VirtualMemoryProtectionScope (IntPtr ptr, nint size, uint oldProtect)
    {
      _ptr = ptr;
      _size = size;
      _oldProtect = oldProtect;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      if (Interlocked.CompareExchange (ref _disposed, 1, 0) != 0)
        return;

      VirtualProtect (_ptr, _size, _oldProtect, out _);
    }
  }
}
