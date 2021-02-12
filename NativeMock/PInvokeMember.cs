namespace NativeMock
{
  using System.Reflection;

  /// <summary>
  /// Represents a P/Invoke method using its native function name and its <see cref="MethodInfo" />.
  /// </summary>
  internal record PInvokeMember(NativeFunctionIdentifier Name, MethodInfo Method);
}
