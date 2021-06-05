namespace NativeMock.Hooking
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using Emit;

  internal class GetProcAddressHook
  {
    private delegate IntPtr GetProcAddressDelegate (IntPtr hModule, IntPtr procName);

#if NET461
    public const string c_clrModuleName = "clr.dll";
#else
    public const string c_clrModuleName = "coreclr.dll";
#endif

    private const string c_kernel32Dll = "kernel32.dll";
    private const string c_getProcAddressName = "GetProcAddress";

    private readonly IHookFactory _hookFactory;
    private readonly IModuleNameResolver _moduleNameResolver;
    private readonly INativeFunctionProxyLookup _nativeFunctionProxyLookup;

    private readonly object _lock = new();

    private bool _isInitialized;
    private int _appDomainId;
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

        var clrModule = Process.GetCurrentProcess()
          .Modules
          .Cast<ProcessModule>()
          .SingleOrDefault (p => p.ModuleName?.Equals (c_clrModuleName, StringComparison.OrdinalIgnoreCase) ?? false);

        if (clrModule == null)
          throw new InvalidOperationException ("Cannot find the CLR module in the current process.");

        _hook = _hookFactory.CreateHook<GetProcAddressDelegate> (clrModule, c_kernel32Dll, c_getProcAddressName, GetProcAddress);
        _appDomainId = AppDomain.CurrentDomain.Id;
        _isInitialized = true;
      }
    }

    public void Destroy()
    {
      _hook.Dispose();
    }

    private IntPtr GetProcAddress (IntPtr module, IntPtr procName)
    {
#if NET461
      // Make sure we do not affect other app domains
      if (AppDomain.CurrentDomain.Id != _appDomainId)
        return _hook.Original (module, procName);
#endif

      var importFunctionName = FunctionName.ParseFromProcName (procName);

      var moduleName = _moduleNameResolver.Resolve (module);
      var nativeFunctionIdentifier = new NativeFunctionIdentifier (moduleName, importFunctionName.ToString());

      return _nativeFunctionProxyLookup.GetNativeFunctionProxy (nativeFunctionIdentifier)?.NativePtr
             ?? _hook.Original (module, procName);
    }
  }
}
