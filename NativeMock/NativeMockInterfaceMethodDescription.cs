namespace NativeMock
{
  using System.Reflection;

  public record NativeMockInterfaceMethodDescription(string FunctionName, MethodInfo MethodInfo)
  {
    public NativeMockCallback CreateCallback (object? target) => new NativeMockCallback (target, MethodInfo);
  }
}
