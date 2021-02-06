namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;

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
        .Select (CreateMockInterfaceMethodDescription)
        .ToImmutableArray();

      if (methodDescriptions.IsEmpty)
        throw new InvalidOperationException ("The specified interface type has no methods.");

      return new NativeMockInterfaceDescription (interfaceType, methodDescriptions);
    }

    private NativeMockInterfaceMethodDescription CreateMockInterfaceMethodDescription (MethodInfo methodInfo)
    {
      var nativeMockFunctionAttribute = methodInfo.GetCustomAttribute<NativeMockCallbackAttribute>();

      var functionName = nativeMockFunctionAttribute?.Name ?? methodInfo.Name;

      var nativeFunctionIdentifier = new NativeFunctionIdentifier (functionName);
      return new NativeMockInterfaceMethodDescription (nativeFunctionIdentifier, methodInfo);
    }
  }
}
