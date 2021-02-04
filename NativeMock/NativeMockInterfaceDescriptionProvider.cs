namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;

  public class NativeMockInterfaceDescriptionProvider : INativeMockInterfaceDescriptionProvider
  {
    /// <inheritdoc />
    public NativeMockInterfaceDescription GetMockInterfaceDescription (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified type must be an interface.");

      var methodDescriptions = interfaceType.GetMethods()
        .Select (m => new NativeMockInterfaceMethodDescription (new NativeFunctionIdentifier (m.Name), m))
        .ToImmutableArray();

      if (methodDescriptions.IsEmpty)
        throw new InvalidOperationException ("The specified interface type has no methods.");

      return new NativeMockInterfaceDescription (interfaceType, methodDescriptions);
    }
  }
}
