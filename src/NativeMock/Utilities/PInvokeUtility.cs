namespace NativeMock.Utilities
{
  using System;
  using System.Diagnostics;
  using System.Runtime.InteropServices;
  using Hooking;

  internal static class PInvokeUtility
  {
    [DllImport ("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandleW ([MarshalAs (UnmanagedType.LPWStr)] string moduleName);

    [DllImport ("Kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
    private static extern IntPtr GetProcAddressNamed (IntPtr hModule, string procName);

    [DllImport ("Kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
    private static extern IntPtr GetProcAddressOrdinal (IntPtr hModule, nint procName);

    public static IntPtr ResolveDllImport (string module, string function)
    {
      if (module == null)
        throw new ArgumentNullException (nameof(module));
      if (function == null)
        throw new ArgumentNullException (nameof(function));

      var moduleHandle = GetModuleHandleW (module);
      if (moduleHandle == IntPtr.Zero)
        moduleHandle = NativeLibrary.Load (module);

      Debug.Assert (moduleHandle != IntPtr.Zero, "moduleHandle != IntPtr.Zero");

      var functionName = FunctionName.ParseFromString (function);
      var result = functionName.IsOrdinal
        ? GetProcAddressOrdinal (moduleHandle, functionName.OrdinalValue)
        : GetProcAddressNamed (moduleHandle, functionName.StringValue);

      if (result == IntPtr.Zero)
        throw new EntryPointNotFoundException ($"Unable to find an entry point named '{function}' in DLL '{module}'.");

      return result;
    }
  }
}
