namespace NativeMock
{
  using System.Reflection;

  public interface INativeMockInterfaceMethodDescriptionProvider
  {
    NativeMockInterfaceMethodDescription GetMockInterfaceDescription (MethodInfo method);
  }
}
