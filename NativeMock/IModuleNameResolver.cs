namespace NativeMock
{
  using System;

  /// <summary>
  /// Provides methods for resolving native module names.
  /// </summary>
  internal interface IModuleNameResolver
  {
    /// <summary>
    /// Resolves the module name from the specified <paramref name="moduleHandle" />.
    /// </summary>
    unsafe string Resolve (IntPtr moduleHandle);
  }
}
