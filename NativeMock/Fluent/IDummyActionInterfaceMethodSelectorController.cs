namespace NativeMock.Fluent
{
  using System.Reflection;

  public interface IDummyActionInterfaceMethodSelectorController
  {
    MethodInfo? GetResult();

    int GetSetCount();

    void Reset();
  }
}