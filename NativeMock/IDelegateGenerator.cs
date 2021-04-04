namespace NativeMock
{
  using System;
  using System.Reflection;

  /// <summary>
  /// Provides methods for dynamically generating delegate types.
  /// </summary>
  internal interface IDelegateGenerator
  {
    /// <summary>
    /// Creates a delegate type from the specified <paramref name="methodInfo" />, retaining any applied custom attributes.
    /// </summary>
    Type CreateDelegateType (MethodInfo methodInfo);
  }
}
