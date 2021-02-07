namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;

  public class NativeMockInterfaceDescriptionProvider : INativeMockInterfaceDescriptionProvider
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

      var nativeMockModuleDescription = _nativeMockModuleDescriptionProvider.GetMockModuleDescription (interfaceType);

      var nativeMockInterfaceAttribute = interfaceType.GetCustomAttribute<NativeMockInterfaceAttribute>();
      var defaultDeclaringType = nativeMockInterfaceAttribute?.DeclaringType;

      var methodDescriptions = methods
        .Select (method => _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (method, defaultDeclaringType))
        .ToImmutableArray();

      return new NativeMockInterfaceDescription (interfaceType, nativeMockModuleDescription, methodDescriptions);
    }
  }
}
