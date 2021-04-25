namespace NativeMock
{
  using System;

  /// <summary>
  /// Specifies how the <see cref="NativeMockRegistry" />.<see cref="NativeMockRegistry.RegisterFromAssembly" /> method
  /// behaves.
  /// </summary>
  [Flags]
  public enum RegisterFromAssemblySearchBehavior
  {
    /// <summary>
    /// Searches for public non-nested types.
    /// </summary>
    Default = 0x0,
    
    /// <summary>
    /// Nested types are searched as suitable native mock interface definitions.
    /// </summary>
    IncludeNestedTypes = 0x1
  }
}
