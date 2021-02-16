namespace NativeMock
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Linq;
  using System.Reflection;

  /// <inheritdoc />
  internal class NativeMockModuleDescriptionProvider : INativeMockModuleDescriptionProvider
  {
    /// <inheritdoc />
    public ImmutableArray<NativeMockModuleDescription> GetMockModuleDescription (Type interfaceType)
    {
      if (interfaceType == null)
        throw new ArgumentNullException (nameof(interfaceType));
      if (!interfaceType.IsInterface)
        throw new InvalidOperationException ("The specified type must be an interface.");

      var nativeMockModuleAttribute = interfaceType.GetCustomAttributes<NativeMockModuleAttribute>()
        .Select (CreateNativeModuleDescription)
        .ToImmutableArray();

      var hashSet = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
      foreach (var nativeMockModuleDescription in nativeMockModuleAttribute)
      {
        if (!hashSet.Add (nativeMockModuleDescription.Name))
          throw new InvalidOperationException ($"The type '{interfaceType}' defines the same native module '{nativeMockModuleDescription.Name}' twice.");
      }

      return nativeMockModuleAttribute;
    }

    private static NativeMockModuleDescription CreateNativeModuleDescription (NativeMockModuleAttribute attribute)
    {
      return new (attribute.ModuleName);
    }
  }
}
