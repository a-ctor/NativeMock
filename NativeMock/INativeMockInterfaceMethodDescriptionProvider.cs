namespace NativeMock
{
  using System;
  using System.Reflection;

  public interface INativeMockInterfaceMethodDescriptionProvider
  {
    NativeMockInterfaceMethodDescription GetMockInterfaceDescription (MethodInfo method, Type? defaultDeclaringType);
  }
}
