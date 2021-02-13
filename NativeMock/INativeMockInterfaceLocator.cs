namespace NativeMock
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  /// <summary>
  /// Provides methods for locating potential mock interface types in an assembly.
  /// </summary>
  internal interface INativeMockInterfaceLocator
  {
    IEnumerable<Type> LocateNativeMockInterfaces (Assembly assembly);
  }
}
