namespace NativeMock
{
  using System.Reflection;

  internal record PInvokeMember(string Name, MethodInfo Method);
}
