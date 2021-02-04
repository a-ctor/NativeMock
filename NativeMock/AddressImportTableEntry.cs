namespace NativeMock
{
  using System.Runtime.InteropServices;

  // see http://sandsprite.com/CodeStuff/Understanding_imports.html
  [StructLayout (LayoutKind.Sequential)]
  internal readonly struct AddressImportTableEntry
  {
    public readonly uint RvaImportLookupTable;
    public readonly uint TimeDateStamp;
    public readonly uint ForwarderChain;
    public readonly int RvaModuleName;
    public readonly int RvaImportAddressTable;
  }
}
