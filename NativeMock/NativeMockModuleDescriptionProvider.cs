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
      return nativeMockModuleAttribute != null
        ? new NativeMockModuleDescription (nativeMockModuleAttribute.ModuleName)
        : null;
    }
  }
}
