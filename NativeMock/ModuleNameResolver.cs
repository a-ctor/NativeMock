namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;
  using System.ComponentModel;
  using System.Runtime.InteropServices;

  public class ModuleNameResolver
  {
    [DllImport ("Kernel32.dll", SetLastError = true)]
    private static extern unsafe int GetModuleFileNameW (IntPtr moduleHandle, byte* filename, int capacity);

    public unsafe ModuleNameResolver()
    {
      // We need to use the function here to ensure that it is resolved before the GetProcAddress hook is in place
      // Otherwise we overflow the stack since the CLR wants to resolve GetModuleFileNameW using GetProcAddress
      // which we hooked and thus want to resolve the module name...
      GetModuleFileNameW (IntPtr.Zero, null, 0);
    }

    private const int c_characterCount = 512;

    private readonly ConcurrentDictionary<IntPtr, string> _resolvedModules = new();
    private readonly byte[] _buffer = new byte [c_characterCount * 2];

    public unsafe string Resolve (IntPtr moduleHandle)
    {
      if (_resolvedModules.TryGetValue (moduleHandle, out var moduleName))
        return moduleName;

      lock (_buffer)
      {
        fixed (byte* ptr = _buffer)
        {
          var written = GetModuleFileNameW (moduleHandle, ptr, c_characterCount);
          if (written == 0 || written == c_characterCount)
            throw new Win32Exception();

          moduleName = Marshal.PtrToStringUni (new IntPtr (ptr), written);
        }

        var lastSlash = moduleName.LastIndexOf ('\\');
        if (lastSlash != -1)
          moduleName = moduleName.Substring (lastSlash + 1);

        return _resolvedModules.GetOrAdd (moduleHandle, moduleName);
      }
    }
  }
}
