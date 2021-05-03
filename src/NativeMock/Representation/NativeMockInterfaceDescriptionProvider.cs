namespace NativeMock.Representation
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;

  /// <inheritdoc />
  internal class NativeMockInterfaceDescriptionProvider : INativeMockInterfaceDescriptionProvider
  {
    private readonly INativeMockInterfaceMethodDescriptionProvider _nativeMockInterfaceMethodDescriptionProvider;

    public NativeMockInterfaceDescriptionProvider (INativeMockInterfaceMethodDescriptionProvider nativeMockInterfaceMethodDescriptionProvider)
    {
      _nativeMockInterfaceMethodDescriptionProvider = nativeMockInterfaceMethodDescriptionProvider;
    }

    /// <inheritdoc />
    public NativeMockInterfaceDescription GetMockInterfaceDescription (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new ArgumentException ("The specified type must be an interface.");
      if (interfaceType.GetInterfaces().Length > 0)
        throw new ArgumentException ("The specified interface type cannot implement other interfaces.");

      var methods = interfaceType.GetMethods();
      if (methods.Length == 0)
        throw new InvalidOperationException ("The specified interface type has no methods.");

      var nativeMockInterfaceAttribute = interfaceType.GetCustomAttribute<NativeMockInterfaceAttribute>();
      if (nativeMockInterfaceAttribute == null)
        throw new InvalidOperationException ("The specified interface does not have a NativeMockInterfaceAttribute applied.");

      var methodDescriptions = methods
        .Select (
          e =>
          {
            return _nativeMockInterfaceMethodDescriptionProvider.GetMockInterfaceDescription (
              nativeMockInterfaceAttribute.DllName,
              e,
              nativeMockInterfaceAttribute.DeclaringType,
              nativeMockInterfaceAttribute.Behavior);
          })
        .ToImmutableArray();

      return new NativeMockInterfaceDescription (interfaceType, methodDescriptions);
    }
  }
}
