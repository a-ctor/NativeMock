namespace NativeMock
{
  /// <summary>
  /// Specifies the behavior of a native mock when called without a setup.
  /// </summary>
  public enum NativeMockBehavior
  {
    /// <summary>
    /// The default native mock behavior, which equals <see cref="Strict" />.
    /// </summary>
    Default,

    /// <summary>
    /// A <see cref="NativeFunctionNotMockedException" /> is thrown when no callback is set up.
    /// </summary>
    Strict,

    /// <summary>
    /// A default value is returned when no callback is set up.
    /// </summary>
    Loose
  }
}
