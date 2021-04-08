namespace NativeMock.Representation
{
  using System.Reflection;

  /// <summary>
  /// A description of a mock interface method.
  /// </summary>
  internal record NativeMockInterfaceMethodDescription(MethodInfo InterfaceMethod, MethodInfo PrototypeMethod, NativeMockBehavior Behavior, NativeFunctionIdentifier Name);
}
