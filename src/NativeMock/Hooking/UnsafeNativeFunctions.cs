namespace NativeMock.Hooking
{
  using System;
  using System.IO;
  using System.Runtime.InteropServices;
  using Emit;

  internal static class UnsafeNativeFunctions
  {
    private const string c_nativeDllName = "NativeMock.Native.dll";
    private const string c_nativeDll86Name = "x86\\" + c_nativeDllName;
    private const string c_nativeDll64Name = "x64\\" + c_nativeDllName;

    [DllImport(c_nativeDllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr NmInitGetProcAddressHook();

    [DllImport(c_nativeDllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void NmRegisterProxy(
      [MarshalAs(UnmanagedType.LPWStr)] string moduleName,
      [MarshalAs(UnmanagedType.LPWStr)] string functionName,
      IntPtr callback);

    [DllImport (c_nativeDllName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void NmDestroyProxy();

    public static unsafe IntPtr LoadAndInitializeNative()
    {
      LoadCorrectNativeModuleIfAvailable();

      return NmInitGetProcAddressHook();
    }

    public static void RegisterProxy(NativeFunctionProxy proxy)
    {
      var moduleName = proxy.Name.ModuleName;
      var functionName = proxy.Name.FunctionName;
      var nativePtr = proxy.NativePtr;
      
      NmRegisterProxy(moduleName, functionName, nativePtr);
    }

    public static void DestroyProxy()
    {
      NmDestroyProxy();
    }

    private static unsafe void LoadCorrectNativeModuleIfAvailable()
    {
      var dllName = sizeof(nint) == 4
        ? c_nativeDll86Name
        : c_nativeDll64Name;

      var assemblyLocation = typeof(UnsafeNativeFunctions).Assembly.Location;
      var fullPath = Path.Combine (Path.GetDirectoryName (assemblyLocation)!, dllName);
      if (!File.Exists (fullPath))
        return;

      NativeLibrary.Load (fullPath);
    }
  }
}
