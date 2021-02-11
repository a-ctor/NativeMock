namespace NativeMock
{
  using System;
  using System.Reflection;

  internal class NativeMockModuleDescriptionProvider : INativeMockModuleDescriptionProvider
  {
    /// <inheritdoc />
    public NativeMockModuleDescription? GetMockModuleDescription (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified type must be an interface.");

      var nativeMockModuleAttribute = interfaceType.GetCustomAttribute<NativeMockModuleAttribute>();
      if (nativeMockModuleAttribute == null)
        return null;

      return new NativeMockModuleDescription (nativeMockModuleAttribute.ModuleName);
    }
  }
}
