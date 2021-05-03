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
    Default = IncludeNestedTypes | IncludePrivateTypes,

    /// <summary>
    /// Nested types are searched as suitable native mock interface definitions.
    /// </summary>
    IncludeNestedTypes = 0x1,

    /// <summary>
    /// Non-public types are included as suitable mock interface definitions.
    /// </summary>
    IncludePrivateTypes = 0x2
  }
}
