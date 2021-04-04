namespace NativeMock
{
  /// <summary>
  /// Specifies how the <see cref="NativeMockRegistry" />.<see cref="NativeMockRegistry.RegisterFromAssembly" /> method
  /// behaves.
  /// </summary>
  public enum RegisterFromAssemblySearchBehavior
  {
    /// <summary>
    /// Only top-level types are searched for suitable native mock interface definitions.
    /// </summary>
    TopLevelTypesOnly,

    /// <summary>
    /// Nested types are searched for suitable native mock interface definitions.
    /// </summary>
    IncludeNestedTypes
  }
}
