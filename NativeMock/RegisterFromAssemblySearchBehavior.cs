namespace NativeMock
{
  /// <summary>
  /// Specifies how the <see cref="NativeMockRepository" />.<see cref="NativeMockRepository.RegisterFromAssembly" /> method
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
