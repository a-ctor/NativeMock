namespace NativeMock
{
  using System;
  using System.Reflection;

  internal interface INativeMockInterfaceMethodDescriptionProvider
  {
    NativeMockInterfaceMethodDescription GetMockInterfaceDescription (MethodInfo method, Type? defaultDeclaringType);
  }
}
