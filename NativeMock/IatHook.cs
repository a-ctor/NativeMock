namespace NativeMock
{
  using System;
  using System.Diagnostics;
  using System.Runtime.InteropServices;

  internal static class IatHook
  {
    private const nint c_addressOfNewExeHeaderFieldOffset = 0x3c;
    private const nint c_importTableOffset32 = 0x80;
    private const nint c_importTableOffset64 = 0x90;
    private const uint c_pageExecuteReadWrite = 0x40;
    private const int c_hintSize = 2;

    [DllImport ("kernel32.dll")]
    private static extern bool VirtualProtect (IntPtr lpAddress, nint dwSize, uint flNewProtect, out uint lpflOldProtect);

    public static unsafe HookedFunction<TDelegate> Create<TDelegate> (ProcessModule module, string targetModuleName, FunctionName targetFunctionName, TDelegate redirect)
      where TDelegate : Delegate
    {
      nint baseAddress = module.BaseAddress;

      var iatEntryPtr = (nint*) GetFunctionPointerForIatEntry (baseAddress, targetModuleName, targetFunctionName);
      if (iatEntryPtr == null)
        throw new InvalidOperationException ($"Cannot find the import table entry for '{targetModuleName}+{targetFunctionName}'.");

      VirtualProtect (new IntPtr (iatEntryPtr), sizeof(nint), c_pageExecuteReadWrite, out var oldProtect);
      var orig = Marshal.GetDelegateForFunctionPointer (new IntPtr (*iatEntryPtr), typeof(TDelegate));
      *iatEntryPtr = Marshal.GetFunctionPointerForDelegate (redirect);
      VirtualProtect (new IntPtr (iatEntryPtr), sizeof(nint), oldProtect, out _);

      return new HookedFunction<TDelegate> ((TDelegate) orig, redirect);
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
