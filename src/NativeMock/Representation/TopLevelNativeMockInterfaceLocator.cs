namespace NativeMock.Representation
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  /// <inheritdoc />
  /// <remarks>
  /// Locates only top-level types as potential mock interfaces.
  /// </remarks>
  internal class TopLevelNativeMockInterfaceLocator : INativeMockInterfaceLocator
  {
    private readonly INativeMockInterfaceIdentifier _nativeMockInterfaceIdentifier;

    public TopLevelNativeMockInterfaceLocator (INativeMockInterfaceIdentifier nativeMockInterfaceIdentifier)
    {
      _nativeMockInterfaceIdentifier = nativeMockInterfaceIdentifier;
    }

    /// <inheritdoc />
    public IEnumerable<Type> LocateNativeMockInterfaces (Assembly assembly)
    {
      return assembly.GetTypes()
        .Where (e => !e.IsNested)
        .Where (_nativeMockInterfaceIdentifier.IsNativeMockInterfaceType);
    }
  }
}
