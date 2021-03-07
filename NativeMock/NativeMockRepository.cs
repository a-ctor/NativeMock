namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Threading;

  /// <summary>
  /// Provides methods for registering mock interface and mocking them during tests.
  /// </summary>
  public static class NativeMockRepository
  {
    private delegate IntPtr GetProcAddressDelegate (IntPtr hModule, IntPtr procName);

    private const string c_coreClrDll = "coreclr.dll";
    private const string c_kernel32Dll = "kernel32.dll";
    private const string c_getProcAddressName = "GetProcAddress";

    private const string c_assemblyName = "NativeFunctionDelegateAssembly";
    private const string c_moduleName = "NativeFunctionDelegateModule";

    private static HookedFunction<GetProcAddressDelegate> s_getProcAddressHook = null!;

    private static readonly object s_initializedLock = new();
    private static bool s_initialized;

    private static readonly INativeMockInterfaceIdentifier s_nativeMockInterfaceIdentifier = new PublicTypesOnlyNativeMockInterfaceIdentifierDecorator (new NativeMockInterfaceIdentifier());
    private static readonly INativeMockInterfaceLocatorFactory s_nativeMockInterfaceLocatorFactory = new NativeMockInterfaceLocatorFactory (s_nativeMockInterfaceIdentifier);

    private static readonly INativeMockInterfaceDescriptionProvider s_nativeMockInterfaceDescriptionProvider = new NativeMockInterfaceDescriptionProvider (
      new NativeMockInterfaceMethodDescriptionProvider (new CachingPInvokeMemberProviderDecorator (new PInvokeMemberProvider())));

    private static readonly DelegateGenerator s_delegateGenerator = new (new AssemblyName (c_assemblyName), c_moduleName);
    private static readonly NativeFunctionProxyFactory s_nativeFunctionProxyFactory = new (s_delegateGenerator, new NativeFunctionProxyCodeGenerator());
    private static readonly NativeFunctionProxyRegistry s_nativeFunctionProxyRegistry = new();

    private static readonly ModuleNameResolver s_moduleNameResolver = new();
    private static readonly AsyncLocal<NativeMockSetupRegistry> s_nativeMockSetupRegistry = new();

    private static readonly ConcurrentDictionary<Type, NativeMockInterfaceDescription> s_registeredInterfaces = new();

    /// <summary>
    /// Initializes the native mock infrastructure. Should be called as early as possible.
    /// </summary>
    /// <remarks>
    /// This method is usually called automatically using a module initializer but it should be called explicitly at the start
    /// of the application.
    /// Native methods that have been jitted before the initialization is completed cannot be mocked.
    /// </remarks>
    public static void Initialize()
    {
      if (s_initialized)
        return;

      lock (s_initializedLock)
      {
        if (s_initialized)
          return;

        var coreClrModule = Process.GetCurrentProcess().Modules
          .Cast<ProcessModule>()
          .SingleOrDefault (p => p.ModuleName?.Equals (c_coreClrDll, StringComparison.OrdinalIgnoreCase) ?? false);

        if (coreClrModule == null)
          throw new InvalidOperationException ("Cannot find the CoreCLR module in the current process.");

        s_getProcAddressHook = IatHook.Create<GetProcAddressDelegate> (coreClrModule, c_kernel32Dll, c_getProcAddressName, GetProcAddress);
        s_initialized = true;
      }
    }

    /// <summary>
    /// Sets up the specified <paramref name="implementation" /> for the specific mock interface
    /// <typeparamref name="TInterface" />.
    /// </summary>
    /// <remarks>
    /// This function operations only in the current thread/task control flow.
    /// </remarks>
    public static void Setup<TInterface> (TInterface implementation)
      where TInterface : class
    {
      if (implementation == null)
        throw new ArgumentNullException (nameof(implementation));
      if (!s_registeredInterfaces.ContainsKey (typeof(TInterface)))
        throw new InvalidOperationException ($"The specified mock interface '{typeof(TInterface)}' was not registered.");

      CheckInitialized();

      var nativeMockSetupRegistry = s_nativeMockSetupRegistry.Value ??= new NativeMockSetupRegistry();
      nativeMockSetupRegistry.Setup (implementation);
    }

    /// <summary>
    /// Removes the setup for the specified <typeparamref name="TInterface" />.
    /// </summary>
    /// <remarks>
    /// This function operations only in the current thread/task control flow.
    /// </remarks>
    public static void Reset<TInterface>()
      where TInterface : class
    {
      if (!s_registeredInterfaces.ContainsKey (typeof(TInterface)))
        throw new InvalidOperationException ($"The specified mock interface '{typeof(TInterface)}' was not registered.");

      CheckInitialized();

      s_nativeMockSetupRegistry.Value?.Reset<TInterface>();
    }

    /// <summary>
    /// Removes all registered setups.
    /// </summary>
    /// <remarks>
    /// This function operations only in the current thread/task control flow.
    /// </remarks>
    public static void ResetAll()
    {
      CheckInitialized();

      s_nativeMockSetupRegistry.Value?.ResetAll();
    }

    /// <summary>
    /// Registers a specific mock interface <typeparamref name="TInterface" /> for future mocking. Should be called as early as
    /// possible.
    /// </summary>
    /// <remarks>
    /// This method should be called early in the application lifecycle and for each native mock interface that will be used.
    /// Native methods that have been jitted before a containing interface is registered cannot be mocked.
    /// </remarks>
    public static void Register<TInterface>()
    {
      var interfaceType = typeof(TInterface);
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type parameter must be a interface.");

      CheckInitialized();

      Register (interfaceType);
    }

    /// <summary>
    /// Registers a specific mock interface <paramref name="interfaceType" /> for future mocking. Should be called as early as
    /// possible.
    /// </summary>
    /// <remarks>
    /// This method should be called early in the application lifecycle and for each native mock interface that will be used.
    /// Native methods that have been jitted before a containing interface is registered cannot be mocked.
    /// </remarks>
    public static void Register (Type interfaceType)
    {
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type parameter must be a interface.");

      CheckInitialized();

      lock (s_nativeFunctionProxyRegistry)
      {
        if (s_registeredInterfaces.ContainsKey (interfaceType))
          throw new InvalidOperationException ($"The specified type '{interfaceType}' is already registered.");

        var interfaceDescription = s_nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (interfaceType);

        foreach (var interfaceMethod in interfaceDescription.Methods)
        {
          var nativeFunctionProxy = s_nativeFunctionProxyFactory.CreateNativeFunctionProxy (interfaceMethod);
          s_nativeFunctionProxyRegistry.Register (nativeFunctionProxy);
        }

        s_registeredInterfaces.TryAdd (interfaceType, interfaceDescription);
      }
    }

    /// <summary>
    /// Registers all suitable native mock interfaces from the specified <paramref name="assembly" />.
    /// The parameter <paramref name="registerFromAssemblySearchBehavior" /> determines where to search for possible
    /// interfaces.
    /// </summary>
    /// <remarks>
    /// A native mock interface is considered suitable if:
    /// <list type="bullet">
    ///   <item>
    ///     <term>It is annotated with a <see cref="NativeMockInterfaceAttribute" /></term>
    ///   </item>
    ///   <item>
    ///     <term>and its visibility is 'public'.</term>
    ///   </item>
    /// </list>
    /// </remarks>
    public static void RegisterFromAssembly (Assembly assembly, RegisterFromAssemblySearchBehavior registerFromAssemblySearchBehavior = RegisterFromAssemblySearchBehavior.TopLevelTypesOnly)
    {
      if (assembly == null)
        throw new ArgumentNullException (nameof(assembly));

      var nativeMockInterfaceLocator = s_nativeMockInterfaceLocatorFactory.CreateMockInterfaceLocator (registerFromAssemblySearchBehavior);
      foreach (var type in nativeMockInterfaceLocator.LocateNativeMockInterfaces (assembly))
        Register (type);
    }

    private static IntPtr GetProcAddress (IntPtr module, IntPtr procName)
    {
      var functionName = FunctionName.ParseFromProcName (procName);
      if (functionName.IsOrdinal)
        return s_getProcAddressHook.Original (module, procName);

      var moduleName = s_moduleNameResolver.Resolve (module);
      var nativeFunctionIdentifier = new NativeFunctionIdentifier (moduleName, functionName.StringValue);

      return s_nativeFunctionProxyRegistry.Resolve (nativeFunctionIdentifier)?.NativePtr
             ?? s_getProcAddressHook.Original (module, procName);
    }

    internal static T? GetMockObject<T>()
      where T : class
    {
      return s_nativeMockSetupRegistry.Value?.GetSetup<T>();
    }

    private static void CheckInitialized()
    {
      if (!s_initialized)
      {
        throw new InvalidOperationException ("NativeMockRepository is not initialized. Call .Initialize as early as possible to ensure that all native calls are proxied.");
      }
    }
  }
}
