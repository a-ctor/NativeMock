namespace NativeMock
{
  using System;
  using System.Reflection;

  /// <summary>
  /// Provides methods for registering mock interface and mocking them during tests.
  /// </summary>
  public static class NativeMockRegistry
  {
    public const string ProxyAssemblyName = "NativeMockDynamicAssembly";
    public const string ProxyAssemblyModuleName = "NativeMockDynamicAssemblyModule";

    private static readonly object s_initializedLock = new();
    private static bool s_initialized;

    private static readonly NativeMockInterfaceRegistry s_nativeMockInterfaceRegistry;
    private static readonly GetProcAddressHook s_getGetProcAddressHook;

    static NativeMockRegistry()
    {
      var nativeMockInterfaceIdentifier = new NativeMockInterfaceIdentifier();
      var nativeMockInterfaceIdentifierWithOnlyPublicTypes = new PublicTypesOnlyNativeMockInterfaceIdentifierDecorator (nativeMockInterfaceIdentifier);
      var nativeMockInterfaceLocatorFactory = new NativeMockInterfaceLocatorFactory (nativeMockInterfaceIdentifierWithOnlyPublicTypes);

      var pInvokeMemberProvider = new PInvokeMemberProvider();
      var pInvokeMemberProviderWithCaching = new CachingPInvokeMemberProviderDecorator (pInvokeMemberProvider);
      var nativeMockInterfaceMethodDescriptionProvider = new NativeMockInterfaceMethodDescriptionProvider (pInvokeMemberProviderWithCaching);
      var nativeMockInterfaceDescriptionProvider = new NativeMockInterfaceDescriptionProvider (nativeMockInterfaceMethodDescriptionProvider);

      var assemblyName = new AssemblyName (ProxyAssemblyName);
      var delegateGenerator = new DelegateGenerator (assemblyName, ProxyAssemblyModuleName);
      var handlerProviderMethod = typeof(NativeMockRegistry).GetMethod (nameof(GetMockObject), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
      var nativeFunctionProxyCodeGenerator = new NativeFunctionProxyCodeGenerator (handlerProviderMethod);
      var nativeFunctionProxyFactory = new NativeFunctionProxyFactory (delegateGenerator, nativeFunctionProxyCodeGenerator);

      s_nativeMockInterfaceRegistry = new NativeMockInterfaceRegistry (
        nativeMockInterfaceLocatorFactory,
        nativeMockInterfaceDescriptionProvider,
        nativeFunctionProxyFactory);

      var iatHookFactory = new IatHookFactory();
      var moduleNameResolver = new ModuleNameResolver();
      s_getGetProcAddressHook = new GetProcAddressHook (
        iatHookFactory,
        moduleNameResolver,
        s_nativeMockInterfaceRegistry);


      var nativeMockSetupInternalRegistryFactory = new NativeMockSetupInternalRegistryFactory();
      LocalSetupsInternal = new AsyncLocalNativeMockSetupRegistry (nativeMockSetupInternalRegistryFactory);
    }

    internal static INativeMockSetupInternalRegistry LocalSetupsInternal { get; }

    public static INativeMockSetupRegistry LocalSetups => LocalSetupsInternal;

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

        s_getGetProcAddressHook.Initialize();

        s_initialized = true;
      }
    }

    /// <summary>
    /// Returns <see langword="true" /> if the type specified by <typeparamref name="TInterface" /> has been registered,
    /// otherwise
    /// returns <see langword="false" />.
    /// </summary>
    public static bool IsRegistered<TInterface>()
      where TInterface : class
    {
      return s_nativeMockInterfaceRegistry.IsRegistered<TInterface>();
    }

    /// <summary>
    /// Returns <see langword="true" /> if the type specified by <paramref name="interfaceType" /> has been registered,
    /// otherwise returns <see langword="false" />.
    /// </summary>
    public static bool IsRegistered (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));

      return s_nativeMockInterfaceRegistry.IsRegistered (interfaceType);
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
      where TInterface : class
    {
      CheckInitialized();

      s_nativeMockInterfaceRegistry.Register<TInterface>();
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
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));

      CheckInitialized();

      s_nativeMockInterfaceRegistry.Register (interfaceType);
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

      CheckInitialized();

      s_nativeMockInterfaceRegistry.RegisterFromAssembly (assembly, registerFromAssemblySearchBehavior);
    }

    internal static T? GetMockObject<T>()
      where T : class
    {
      return LocalSetupsInternal.GetSetup<T>();
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