namespace NativeMock
{
  using System.Reflection;

  /// <summary>
  /// A description of a mock interface method.
  /// </summary>
  internal record NativeMockInterfaceMethodDescription(string FunctionName, NativeMockModuleDescription? Module, MethodInfo InterfaceMethod, MethodInfo StubTargetMethod)
  {
    public NativeMockCallback CreateCallback (object? target) => new (target, InterfaceMethod);

    public NativeFunctionIdentifier CreateNativeFunctionIdentifier()
    {
      return Module != null
        ? new NativeFunctionIdentifier (Module.Name, FunctionName)
        : new NativeFunctionIdentifier (FunctionName);
    }
  }
}
