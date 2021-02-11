namespace NativeMock
{
  using System;
  using System.Collections.Immutable;

  internal record NativeMockInterfaceDescription(Type InterfaceType, NativeMockModuleDescription? Module, ImmutableArray<NativeMockInterfaceMethodDescription> Methods)
  {
    public NativeFunctionIdentifier CreateNativeFunctionIdentifier (NativeMockInterfaceMethodDescription method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      return Module != null
        ? new NativeFunctionIdentifier (Module.Name, method.FunctionName)
        : new NativeFunctionIdentifier (method.FunctionName);
    }
  }
}
