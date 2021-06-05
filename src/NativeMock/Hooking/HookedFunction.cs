namespace NativeMock.Hooking
{
  using System;
  using System.Runtime.CompilerServices;
  using System.Threading;

  /// <summary>
  /// Represents a hooked function using the original and the hooked <typeparamref name="TDelegate" />.
  /// </summary>
  internal class HookedFunction<TDelegate> : IDisposable
  {
    private readonly IntPtr _location;

    private int _disposed;

    public TDelegate Original { get; }

    public IntPtr OriginalAddress { get; }

    public TDelegate Hook { get; }

    public IntPtr HookAddress { get; }

    public HookedFunction (IntPtr location, TDelegate original, IntPtr originalAddress, TDelegate hook, IntPtr hookAddress)
    {
      _location = location;
      Original = original;
      OriginalAddress = originalAddress;
      Hook = hook;
      HookAddress = hookAddress;
    }

    /// <inheritdoc />
    public unsafe void Dispose()
    {
      if (Interlocked.CompareExchange (ref _disposed, 1, 0) != 0)
        return;

      var ptr = (nint*) _location.ToPointer();
      using (VirtualMemoryProtectionScope.SetReadWrite (_location, sizeof(nint)))
        *ptr = OriginalAddress;
    }
  }
}
