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
  public static class NativeMockRegistry
  {
    private delegate IntPtr GetProcAddressDelegate (IntPtr hModule, string procName);

    private const string c_coreClrDll = "coreclr.dll";
    private const string c_kernel32Dll = "kernel32.dll";
    private const string c_getProcAddressName = "GetProcAddress";

    private const string c_assemblyName = "NativeFunctionDelegateAssembly";
    private const string c_moduleName = "NativeFunctionDelegateModule";

    private static HookedFunction<GetProcAddressDelegate> s_getProcAddressHook = null!;

    private static readonly object s_initializedLock = new();
    private static bool s_initialized;

    private static readonly INativeMockInterfaceDescriptionProvider s_nativeMockInterfaceDescriptionProvider = new NativeMockInterfaceDescriptionProvider (
      new NativeMockModuleDescriptionProvider(),
      new NativeMockInterfaceMethodDescriptionProvider (new CachingPInvokeMemberProviderDecorator (new PInvokeMemberProvider())));

    private static readonly DelegateGenerator s_delegateGenerator = new (new AssemblyName (c_assemblyName), c_moduleName);
    private static readonly NativeFunctionProxyFactory s_nativeFunctionProxyFactory = new (OnNativeHookCalled);
    private static readonly NativeFunctionProxyRegistry s_nativeFunctionProxyRegistry = new();

    private static readonly ModuleNameResolver s_moduleNameResolver = new();
    private static readonly AsyncLocal<NativeMockCallbackRegistry> s_nativeMockCallbackRegistry = new();

    private static readonly ConcurrentDictionary<Type, NativeMockInterfaceDescription> s_registeredInterfaces = new();

    /// <summary>
    /// Automatically registers all suitable native mock interfaces from the specified <paramref name="assembly" />.
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
    public static void AutoRegister (Assembly assembly)
    {
      foreach (var type in assembly.DefinedTypes)
      {
        if (!type.IsInterface || !type.IsPublic)
          continue;

        var nativeMockInterfaceAttribute = type.GetCustomAttribute<NativeMockInterfaceAttribute>();
        if (nativeMockInterfaceAttribute == null)
          continue;

        Register (type);
      }
    }

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
    /// Clears all mocks that currently registered.
    /// </summary>
    /// <remarks>
    /// This function operations only in the current thread/task control flow.
    /// </remarks>
    public static void ClearMocks()
    {
      CheckInitialized();

      s_nativeMockCallbackRegistry.Value?.Clear();
    }

    /// <summary>
    /// Registers an <paramref name="implementation" /> for a specific mock interface <typeparamref name="TInterface" />.
    /// </summary>
    /// <remarks>
    /// This function operations only in the current thread/task control flow.
    /// </remarks>
    public static void Mock<TInterface> (TInterface implementation)
    {
      if (implementation == null)
        throw new ArgumentNullException (nameof(implementation));
      if (!s_registeredInterfaces.TryGetValue (typeof(TInterface), out var interfaceDescription))
        throw new InvalidOperationException ($"The specified type '{typeof(TInterface)}' was not registered.");

      CheckInitialized();

      var nativeMockCallbackRegistry = s_nativeMockCallbackRegistry.Value ??= new NativeMockCallbackRegistry();
      foreach (var interfaceMethod in interfaceDescription.Methods)
      {
        var nativeFunctionIdentifier = interfaceDescription.CreateNativeFunctionIdentifier (interfaceMethod);
        nativeMockCallbackRegistry.Register (nativeFunctionIdentifier, interfaceMethod.CreateCallback (implementation));
      }
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
          throw new InvalidOperationException ("The specified type is already registered.");

        var interfaceDescription = s_nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (interfaceType);

        foreach (var interfaceMethod in interfaceDescription.Methods)
        {
          var delegateType = s_delegateGenerator.CreateDelegateType (interfaceMethod.StubTargetMethod);
          var nativeFunctionIdentifier = interfaceDescription.CreateNativeFunctionIdentifier (interfaceMethod);
          var nativeFunctionProxy = s_nativeFunctionProxyFactory.CreateNativeFunctionProxy (nativeFunctionIdentifier, delegateType);
          s_nativeFunctionProxyRegistry.Register (nativeFunctionProxy);
        }

        s_registeredInterfaces.TryAdd (interfaceType, interfaceDescription);
      }
    }

    private static IntPtr GetProcAddress (IntPtr module, string functionName)
    {
      var moduleName = s_moduleNameResolver.Resolve (module);
      var nativeFunctionIdentifier = new NativeFunctionIdentifier (moduleName, functionName);

      return s_nativeFunctionProxyRegistry.Resolve (nativeFunctionIdentifier)?.NativePtr
             ?? s_getProcAddressHook.Original (module, functionName);
    }

    private static object? OnNativeHookCalled (NativeFunctionIdentifier nativeFunctionIdentifier, object?[] args)
    {
      var nativeMockCallbackRegistry = s_nativeMockCallbackRegistry.Value ??= new NativeMockCallbackRegistry();
      return nativeMockCallbackRegistry.Invoke (nativeFunctionIdentifier, args);
    }

    private static void CheckInitialized()
    {
      if (!s_initialized)
      {
        throw new InvalidOperationException ("NativeMockRegistry is not initialized. Call .Initialize as early as possible to ensure that all native calls are proxied.");
      }
    }
  }
}
