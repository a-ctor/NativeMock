namespace NativeMock
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
        throw new InvalidOperationException ("The specified type must be an interface.");

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
              nativeMockInterfaceAttribute.Module,
              e,
              nativeMockInterfaceAttribute.DeclaringType,
              nativeMockInterfaceAttribute.Behavior);
          })
        .ToImmutableArray();

      return new NativeMockInterfaceDescription (interfaceType, methodDescriptions);
    }
  }
}
