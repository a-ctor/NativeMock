namespace NativeMock.Emit
{
  using System;
  using System.Reflection;

  internal interface IDelegateFactory
  {
    Type CreateDelegateType (MethodInfo methodInfo);
  }
}
