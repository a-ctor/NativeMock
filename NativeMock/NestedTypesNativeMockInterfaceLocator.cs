namespace NativeMock
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  /// <inheritdoc />
  /// <remarks>
  /// Locates only top-level types and their nested types as potential mock interfaces.
  /// </remarks>
  internal class NestedTypesNativeMockInterfaceLocator : INativeMockInterfaceLocator
  {
    private readonly INativeMockInterfaceIdentifier _nativeMockInterfaceIdentifier;

    public NestedTypesNativeMockInterfaceLocator (INativeMockInterfaceIdentifier nativeMockInterfaceIdentifier)
    {
      _nativeMockInterfaceIdentifier = nativeMockInterfaceIdentifier;
    }

    /// <inheritdoc />
    public IEnumerable<Type> LocateNativeMockInterfaces (Assembly assembly)
    {
      return assembly.GetTypes()
        .Where (_nativeMockInterfaceIdentifier.IsNativeMockInterfaceType);
    }
  }
}
