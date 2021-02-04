namespace NativeMock
{
  using System.Reflection;

  public record NativeMockInterfaceMethodDescription(NativeFunctionIdentifier Name, MethodInfo MethodInfo)
  {
    public NativeMockCallback CreateCallback (object? target) => new NativeMockCallback (target, MethodInfo);
  }
}
