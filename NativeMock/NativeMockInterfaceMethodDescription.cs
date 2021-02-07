namespace NativeMock
{
  using System.Reflection;

  public record NativeMockInterfaceMethodDescription(string FunctionName, MethodInfo InterfaceMethod, MethodInfo StubTargetMethod)
  {
    public NativeMockCallback CreateCallback (object? target) => new (target, InterfaceMethod);
  }
}
