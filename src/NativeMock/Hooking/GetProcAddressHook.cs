namespace NativeMock.Hooking
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;

  internal class GetProcAddressHook
  {
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr GetProcAddressDelegate (IntPtr hModule, IntPtr procName);

#if NETFRAMEWORK
    private const string c_clrModuleName = "clr.dll";
#else
    private const string c_clrModuleName = "coreclr.dll";
#endif

    private const string c_kernel32Dll = "kernel32.dll";
    private const string c_getProcAddressName = "GetProcAddress";

    private readonly IHookFactory _hookFactory;

    private readonly object _lock = new();

    private bool _isInitialized;
    private HookedFunction<GetProcAddressDelegate> _hook = null!;

    public GetProcAddressHook (IHookFactory hookFactory)
    {
      if (hookFactory == null)
        throw new ArgumentNullException (nameof(hookFactory));

      _hookFactory = hookFactory;
    }

    public void Initialize()
    {
      if (_isInitialized)
        return;

      lock (_lock)
      {
        if (_isInitialized)
          return;

        var clrModule = Process.GetCurrentProcess()
          .Modules
          .Cast<ProcessModule>()
          .SingleOrDefault (p => p.ModuleName?.Equals (c_clrModuleName, StringComparison.OrdinalIgnoreCase) ?? false);

        if (clrModule == null)
          throw new InvalidOperationException ("Cannot find the CLR module in the current process.");

        var hookAddress = UnsafeNativeFunctions.LoadAndInitializeNative();
        
        _hook = _hookFactory.CreateHook<GetProcAddressDelegate> (clrModule, c_kernel32Dll, c_getProcAddressName, hookAddress);
        _isInitialized = true;
      }
    }

    public void Destroy()
    {
      _hook.Dispose();
      UnsafeNativeFunctions.DestroyProxy();
    }
  }
}
