namespace NativeMock.Emit
{
  using System;

  internal interface IDummyActionInterfaceMethodSelectorCodeGenerator
  {
    Type CreateDummyActionInterfaceMethodSelector (Type interfaceType);
  }
}
