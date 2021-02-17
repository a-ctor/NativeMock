namespace NativeMock
{
  using System.Reflection;

  /// <summary>
  /// A description of a mock interface method.
  /// </summary>
  internal record NativeMockInterfaceMethodDescription(NativeFunctionIdentifier Name, MethodInfo InterfaceMethod, MethodInfo StubTargetMethod, NativeMockBehavior Behavior)
  {
    public NativeMockCallback CreateCallback (object? target) => new (target, InterfaceMethod);
  }
}
