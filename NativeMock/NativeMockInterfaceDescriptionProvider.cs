namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;

  public class NativeMockInterfaceDescriptionProvider : INativeMockInterfaceDescriptionProvider
  {
    private record InterfaceSettings(string? Module)
    {
      public static readonly InterfaceSettings Default = new ((string?) null);
    }

    private record InterfaceMethodSettings(string? Name)
    {
      public static readonly InterfaceMethodSettings Default = new ((string?) null);
    }

    /// <inheritdoc />
    public NativeMockInterfaceDescription GetMockInterfaceDescription (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified type must be an interface.");

      var methodDescriptions = CreateMockInterfaceMethodDescriptions (interfaceType);
      if (methodDescriptions.IsEmpty)
        throw new InvalidOperationException ("The specified interface type has no methods.");

      return new NativeMockInterfaceDescription (interfaceType, methodDescriptions);
    }

    private ImmutableArray<NativeMockInterfaceMethodDescription> CreateMockInterfaceMethodDescriptions (Type interfaceType)
    {
      var interfaceSettings = GetInterfaceSettings (interfaceType);
      return interfaceType.GetMethods()
        .Select (method => CreateMockInterfaceMethodDescription (method, interfaceSettings))
        .ToImmutableArray();
    }

    private InterfaceSettings GetInterfaceSettings (Type type)
    {
      var nativeMockInterfaceAttribute = type.GetCustomAttribute<NativeMockInterfaceAttribute>();
      if (nativeMockInterfaceAttribute == null)
        return InterfaceSettings.Default;

      return new InterfaceSettings (nativeMockInterfaceAttribute.Module);
    }

    private NativeMockInterfaceMethodDescription CreateMockInterfaceMethodDescription (MethodInfo methodInfo, InterfaceSettings interfaceSettings)
    {
      var interfaceMethodSettings = GetInterfaceMethodSettings (methodInfo);

      var moduleName = interfaceSettings.Module;
      var functionName = interfaceMethodSettings.Name ?? methodInfo.Name;

      var nativeFunctionIdentifier = moduleName != null
        ? new NativeFunctionIdentifier (moduleName, functionName)
        : new NativeFunctionIdentifier (functionName);

      return new NativeMockInterfaceMethodDescription (nativeFunctionIdentifier, methodInfo);
    }

    private InterfaceMethodSettings GetInterfaceMethodSettings (MethodInfo methodInfo)
    {
      var nativeMockCallbackAttribute = methodInfo.GetCustomAttribute<NativeMockCallbackAttribute>();
      if (nativeMockCallbackAttribute == null)
        return InterfaceMethodSettings.Default;

      return new InterfaceMethodSettings (nativeMockCallbackAttribute.Name);
    }
  }
}
