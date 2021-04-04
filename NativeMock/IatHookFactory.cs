namespace NativeMock
{
  using System;
  using System.Diagnostics;
  using System.Runtime.InteropServices;

  /// <summary>
  /// Provides methods for hooking imported functions by modifying the address import table of the module's PE header.
  /// </summary>
  internal class IatHookFactory : IHookFactory
  {
    private const nint c_addressOfNewExeHeaderFieldOffset = 0x3c;
    private const nint c_importTableOffset32 = 0x80;
    private const nint c_importTableOffset64 = 0x90;
    private const uint c_pageExecuteReadWrite = 0x40;
    private const int c_hintSize = 2;

    [DllImport ("kernel32.dll")]
    private static extern bool VirtualProtect (IntPtr lpAddress, nint dwSize, uint flNewProtect, out uint lpflOldProtect);

    /// <summary>
    /// Creates a hook for an imported function using <paramref name="hook" />
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type describing the signature of the target function.</typeparam>
    /// <param name="module">The target module whose imported function should be hooked.</param>
    /// <param name="targetModuleName">The module name (e.g. kernel32.dll) of the module containing the function to be hooked.</param>
    /// <param name="targetFunctionName">The name of the function to be hooked.</param>
    /// <param name="hook">The delegate that will be called instead of the original function.</param>
    /// <returns>
    /// Returns a <see cref="HookedFunction{TDelegate}" /> which contains the original function that has been replaced
    /// by the hook.
    /// </returns>
    public unsafe HookedFunction<TDelegate> CreateHook<TDelegate> (ProcessModule module, string targetModuleName, FunctionName targetFunctionName, TDelegate hook)
      where TDelegate : Delegate
    {
      nint baseAddress = module.BaseAddress;

      var iatEntryPtr = (nint*) GetFunctionPointerForIatEntry (baseAddress, targetModuleName, targetFunctionName);
      if (iatEntryPtr == null)
        throw new InvalidOperationException ($"Cannot find the import table entry for '{targetModuleName}+{targetFunctionName}'.");

      VirtualProtect (new IntPtr (iatEntryPtr), sizeof(nint), c_pageExecuteReadWrite, out var oldProtect);
      var orig = Marshal.GetDelegateForFunctionPointer (new IntPtr (*iatEntryPtr), typeof(TDelegate));
      *iatEntryPtr = Marshal.GetFunctionPointerForDelegate (hook);
      VirtualProtect (new IntPtr (iatEntryPtr), sizeof(nint), oldProtect, out _);

      return new HookedFunction<TDelegate> ((TDelegate) orig, hook);
    }

    private static unsafe nint GetFunctionPointerForIatEntry (nint moduleBaseAddress, string targetModuleName, FunctionName targetFunctionName)
    {
      var correctImportTableOffset = sizeof(nint) == 4 ? c_importTableOffset32 : c_importTableOffset64;
      var iat = (int*) (*(int*) (moduleBaseAddress + c_addressOfNewExeHeaderFieldOffset) + moduleBaseAddress + correctImportTableOffset);
      var rva = iat[0]; // this should actually be 1 so the previous offset is most likely skewed

      var importTable = (AddressImportTableEntry*) (moduleBaseAddress + rva);
      for (var i = 0; importTable[i].RvaImportLookupTable != 0; i++)
      {
        var importTableEntry = importTable[i];
        var importedModuleName = Marshal.PtrToStringAnsi (moduleBaseAddress + importTableEntry.RvaModuleName);
        if (importedModuleName == null || !importedModuleName.Equals (targetModuleName, StringComparison.OrdinalIgnoreCase))
          continue;

        var iltEntry = (AddressImportTableImportIdentifier*) (moduleBaseAddress + importTableEntry.RvaImportLookupTable);
        for (var j = 0; !iltEntry[j].IsEmpty; j++)
        {
          if (iltEntry[j].IsOrdinal)
          {
            if (iltEntry[j].Value != targetFunctionName.OrdinalValue)
              continue;
          }
          else
          {
            var importFunctionName = Marshal.PtrToStringAnsi (moduleBaseAddress + iltEntry[j].Value + c_hintSize)!;
            if (!importFunctionName.Equals (targetFunctionName.StringValue, StringComparison.CurrentCultureIgnoreCase))
              continue;
          }

          return moduleBaseAddress + importTableEntry.RvaImportAddressTable + sizeof(nint) * j;
        }
      }

      return 0;
    }
  }
}
