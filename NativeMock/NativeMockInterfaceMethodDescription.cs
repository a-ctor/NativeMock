namespace NativeMock
{
  using System.Reflection;

  /// <summary>
  /// A description of a mock interface method.
  /// </summary>
  internal record NativeMockInterfaceMethodDescription(string FunctionName, MethodInfo InterfaceMethod, MethodInfo StubTargetMethod)
  {
    public NativeMockCallback CreateCallback (object? target) => new (target, InterfaceMethod);
  }
}
