namespace NativeMock.Registration
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Threading;
  using Emit;
  using Representation;

  internal class NativeMockInterfaceRegistry : INativeFunctionProxyLookup, INativeMockInterfaceDescriptionLookup
  {
    private readonly INativeMockInterfaceLocatorFactory _nativeMockInterfaceLocatorFactory;
    private readonly INativeMockInterfaceDescriptionProvider _nativeMockInterfaceDescriptionProvider;
    private readonly INativeFunctionProxyFactory _nativeFunctionProxyFactory;

    private readonly ReaderWriterLockSlim _readerWriterLock = new (LockRecursionPolicy.SupportsRecursion);
    private readonly Dictionary<Type, NativeMockInterfaceDescription> _registeredInterfaces = new();
    private readonly Dictionary<NativeFunctionIdentifier, NativeFunctionProxy> _nativeFunctionProxies = new();

    public NativeMockInterfaceRegistry (
      INativeMockInterfaceLocatorFactory nativeMockInterfaceLocatorFactory,
      INativeMockInterfaceDescriptionProvider nativeMockInterfaceDescriptionProvider,
      INativeFunctionProxyFactory nativeFunctionProxyFactory)
    {
      if (nativeMockInterfaceLocatorFactory == null)
        throw new ArgumentNullException (nameof(nativeMockInterfaceLocatorFactory));
      if (nativeMockInterfaceDescriptionProvider == null)
        throw new ArgumentNullException (nameof(nativeMockInterfaceDescriptionProvider));
      if (nativeFunctionProxyFactory == null)
        throw new ArgumentNullException (nameof(nativeFunctionProxyFactory));

      _nativeMockInterfaceLocatorFactory = nativeMockInterfaceLocatorFactory;
      _nativeMockInterfaceDescriptionProvider = nativeMockInterfaceDescriptionProvider;
      _nativeFunctionProxyFactory = nativeFunctionProxyFactory;
    }

    public bool IsRegistered (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type parameter must be a interface.");

      _readerWriterLock.EnterReadLock();
      try
      {
        return _registeredInterfaces.ContainsKey (interfaceType);
      }
      finally
      {
        _readerWriterLock.ExitReadLock();
      }
    }

    public void Register (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type parameter must be a interface.");
      if (interfaceType.GetInterfaces().Length > 0)
        throw new ArgumentException ("The specified interface type cannot implement other interfaces.");

      _readerWriterLock.EnterWriteLock();
      try
      {
        RegisterInternal (interfaceType);
      }
      finally
      {
        _readerWriterLock.ExitWriteLock();
      }
    }

    public void RegisterFromAssembly (Assembly assembly, RegisterFromAssemblySearchBehavior registerFromAssemblySearchBehavior)
    {
      if (assembly == null)
        throw new ArgumentNullException (nameof(assembly));

      _readerWriterLock.EnterWriteLock();
      try
      {
        var nativeMockInterfaceLocator = _nativeMockInterfaceLocatorFactory.CreateMockInterfaceLocator (registerFromAssemblySearchBehavior);
        foreach (var type in nativeMockInterfaceLocator.LocateNativeMockInterfaces (assembly))
          Register (type);
      }
      finally
      {
        _readerWriterLock.ExitWriteLock();
      }
    }

    private void RegisterInternal (Type interfaceType)
    {
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type parameter must be a interface.");
      if (_registeredInterfaces.ContainsKey (interfaceType))
        throw new InvalidOperationException ($"The specified type '{interfaceType}' is already registered.");

      // 1. Generate all proxies - this ensures that all the proxies have been generated before registering
      var interfaceDescription = _nativeMockInterfaceDescriptionProvider.GetMockInterfaceDescription (interfaceType);
      var nativeFunctionProxies = interfaceDescription.Methods
        .Select (_nativeFunctionProxyFactory.CreateNativeFunctionProxy)
        .ToArray();

      // 2. Make sure that none of the native functions already have a proxy assigned to them
      var alreadyRegisteredFunctionProxies = nativeFunctionProxies
        .Where (e => _nativeFunctionProxies.ContainsKey (e.Name))
        .Select (e => $"'{e.Name}'")
        .ToArray();
      if (alreadyRegisteredFunctionProxies.Length != 0)
      {
        var alreadyRegisteredMethods = string.Join (",", alreadyRegisteredFunctionProxies);
        throw new InvalidOperationException ($"The methods {alreadyRegisteredMethods} of the specified type '{interfaceType}' conflict with already registered methods.");
      }

      // 3. Register the native function proxies since we are now sure that there are no conflicts
      foreach (var nativeFunctionProxy in nativeFunctionProxies)
        _nativeFunctionProxies.Add (nativeFunctionProxy.Name, nativeFunctionProxy);
      _registeredInterfaces.Add (interfaceType, interfaceDescription);
    }

    public NativeFunctionProxy? GetNativeFunctionProxy (NativeFunctionIdentifier nativeFunctionIdentifier)
    {
      if (nativeFunctionIdentifier.IsInvalid)
        throw new ArgumentNullException (nameof(nativeFunctionIdentifier));

      return _nativeFunctionProxies.TryGetValue (nativeFunctionIdentifier, out var proxy)
        ? proxy
        : null;
    }

    /// <inheritdoc />
    public NativeMockInterfaceDescription? GetMockInterfaceDescription<T>()
      where T : class
    {
      return _registeredInterfaces.TryGetValue (typeof(T), out var description) ? description : null;
    }
  }
}
