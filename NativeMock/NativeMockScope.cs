namespace NativeMock
{
  using System.Threading;

  /// <summary>
  /// Defines scopes in which a native mock is registered in.
  /// </summary>
  public enum NativeMockScope
  {
    /// <summary>
    /// The default scope is <see cref="Local" />.
    /// </summary>
    Default,

    /// <summary>
    /// The native mock is registered in a <see cref="AsyncLocal{T}" /> environment and will flow to new threads/tasks.
    /// A local mock will also override any registered mock in the global scope.
    /// </summary>
    /// <remarks>
    /// The native mock will flow to created threads and tasks according to <see cref="AsyncLocal{T}" />.
    /// </remarks>
    /// <seealso cref="AsyncLocal{T}" />
    Local,

    /// <summary>
    /// The native mock is registered in the global environment and is used by all threads.
    /// A global native mock can be overriden by a local native mock.
    /// </summary>
    Global
  }
}
