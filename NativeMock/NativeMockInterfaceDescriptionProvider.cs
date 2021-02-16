namespace NativeMock
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;

  /// <inheritdoc />
  internal class NativeMockInterfaceDescriptionProvider : INativeMockInterfaceDescriptionProvider
  {
    private readonly INativeMockModuleDescriptionProvider _nativeMockModuleDescriptionProvider;
    private readonly INativeMockInterfaceMethodDescriptionProvider _nativeMockInterfaceMethodDescriptionProvider;

    public NativeMockInterfaceDescriptionProvider (
      INativeMockModuleDescriptionProvider nativeMockModuleDescriptionProvider,
      INativeMockInterfaceMethodDescriptionProvider nativeMockInterfaceMethodDescriptionProvider)
    {
      _nativeMockModuleDescriptionProvider = nativeMockModuleDescriptionProvider;
      _nativeMockInterfaceMethodDescriptionProvider = nativeMockInterfaceMethodDescriptionProvider;
    }

    /// <inheritdoc />
    public NativeMockInterfaceDescription GetMockInterfaceDescription (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified type must be an interface.");

      var methods = interfaceType.GetMethods();
      if (methods.Length == 0)
        throw new InvalidOperationException ("The specified interface type has no methods.");

      var defaultDeclaringType = interfaceType.GetCustomAttribute<NativeMockInterfaceAttribute>()?.DeclaringType;
      var defaultModuleDescriptions = _nativeMockModuleDescriptionProvider.GetMockModuleDescription (interfaceType);

      // With no module-attributes we create unscoped functions otherwise we create a new function for each module
      var methodDescriptionsEnumerable = defaultModuleDescriptions.IsEmpty
        ? GetNativeMockInterfaceMethodDescriptions (methods, defaultDeclaringType, null)
        : defaultModuleDescriptions.SelectMany (e => GetNativeMockInterfaceMethodDescriptions (methods, defaultDeclaringType, e));

      return new NativeMockInterfaceDescription (interfaceType, methodDescriptionsEnumerable.ToImmutableArray());
    }

    private IEnumerable<NativeMockInterfaceMethodDescription> GetNativeMockInterfaceMethodDescriptions (
      IEnumerable<MethodInfo> methods,
      Type? defaultDeclaringType,
      NativeMockModuleDescription? defaultModule)
    {
      return methods.Select (e => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (e, defaultDeclaringType, defaultModule?.Name));
    }
  }
}
