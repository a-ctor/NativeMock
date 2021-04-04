namespace NativeMock
{
  using System;
  using System.Diagnostics;

  /// <summary>
  /// Provides methods for hooking functions.
  /// </summary>
  internal interface IHookFactory
  {
    unsafe HookedFunction<TDelegate> CreateHook<TDelegate> (ProcessModule module, string targetModuleName, FunctionName targetFunctionName, TDelegate hook)
      where TDelegate : Delegate;
  }
}
