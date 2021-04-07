namespace NativeMock.Hooking
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using Emit;

  internal class GetProcAddressHook
  {
    private delegate IntPtr GetProcAddressDelegate (IntPtr hModule, IntPtr procName);

    private const string c_coreClrDll = "coreclr.dll";
    private const string c_kernel32Dll = "kernel32.dll";
    private const string c_getProcAddressName = "GetProcAddress";

    private readonly IHookFactory _hookFactory;
    private readonly IModuleNameResolver _moduleNameResolver;
    private readonly INativeFunctionProxyLookup _nativeFunctionProxyLookup;

    private readonly object _lock = new();

    private bool _isInitialized;
    private HookedFunction<GetProcAddressDelegate> _hook = null!;

    public GetProcAddressHook (
      IHookFactory hookFactory,
      IModuleNameResolver moduleNameResolver,
      INativeFunctionProxyLookup nativeFunctionProxyLookup)
    {
      if (hookFactory == null)
        throw new ArgumentNullException (nameof(hookFactory));
      if (moduleNameResolver == null)
        throw new ArgumentNullException (nameof(moduleNameResolver));
      if (nativeFunctionProxyLookup == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxyLookup));

      _hookFactory = hookFactory;
      _moduleNameResolver = moduleNameResolver;
      _nativeFunctionProxyLookup = nativeFunctionProxyLookup;
    }

    public void Initialize()
    {
      if (_isInitialized)
        return;

      lock (_lock)
      {
        if (_isInitialized)
          return;

        var coreClrModule = Process.GetCurrentProcess().Modules
          .Cast<ProcessModule>()
          .SingleOrDefault (p => p.ModuleName?.Equals (c_coreClrDll, StringComparison.OrdinalIgnoreCase) ?? false);

        if (coreClrModule == null)
          throw new InvalidOperationException ("Cannot find the CoreCLR module in the current process.");

        _hook = _hookFactory.CreateHook<GetProcAddressDelegate> (coreClrModule, c_kernel32Dll, c_getProcAddressName, GetProcAddress);
        _isInitialized = true;
      }
    }

    private IntPtr GetProcAddress (IntPtr module, IntPtr procName)
    {
      var functionName = FunctionName.ParseFromProcName (procName);
      if (functionName.IsOrdinal)
        return _hook.Original (module, procName);

      var moduleName = _moduleNameResolver.Resolve (module);
      var nativeFunctionIdentifier = new NativeFunctionIdentifier (moduleName, functionName.StringValue);

      return _nativeFunctionProxyLookup.GetNativeFunctionProxy (nativeFunctionIdentifier)?.NativePtr
             ?? _hook.Original (module, procName);
    }
  }
}
