namespace NativeMock.Fluent
{
  using System.Reflection;

  internal interface IDummyActionInterfaceMethodSelectorController
  {
    MethodInfo? GetResult();

    int GetSetCount();

    void Reset();
  }
}
