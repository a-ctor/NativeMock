namespace NativeMock
{
  using System.Reflection;

  public record PInvokeMember(string Name, MethodInfo Method);
}
