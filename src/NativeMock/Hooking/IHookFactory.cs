namespace NativeMock.Hooking
{
  using System;
  using System.Diagnostics;

  /// <summary>
  /// Provides methods for hooking functions.
  /// </summary>
  internal interface IHookFactory
  {
    unsafe HookedFunction<TDelegate> CreateHook<TDelegate> (ProcessModule module, string targetModuleName, FunctionName targetFunctionName, IntPtr hook)
      where TDelegate : Delegate;
  }
}
