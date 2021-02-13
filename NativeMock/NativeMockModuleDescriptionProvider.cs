namespace NativeMock
{
  using System;
  using System.Reflection;

  /// <inheritdoc />
  internal class NativeMockModuleDescriptionProvider : INativeMockModuleDescriptionProvider
  {
    /// <inheritdoc />
    public NativeMockModuleDescription? GetMockModuleDescriptionForType (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified type must be an interface.");

      var nativeMockModuleAttribute = interfaceType.GetCustomAttribute<NativeMockModuleAttribute>();
      return CreateForAttribute (nativeMockModuleAttribute);
    }

    /// <inheritdoc />
    public NativeMockModuleDescription? GetMockModuleDescriptionForMethod (MethodInfo method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      var nativeMockModuleAttribute = method.GetCustomAttribute<NativeMockModuleAttribute>();
      return CreateForAttribute (nativeMockModuleAttribute);
    }

    private NativeMockModuleDescription? CreateForAttribute (NativeMockModuleAttribute? nativeMockModuleAttribute)
    {
      return nativeMockModuleAttribute != null
        ? new NativeMockModuleDescription (nativeMockModuleAttribute.ModuleName)
        : null;
    }
  }
}
