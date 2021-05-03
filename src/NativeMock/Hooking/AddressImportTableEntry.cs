namespace NativeMock.Hooking
{
  using System.Runtime.InteropServices;

  /// <summary>
  /// Represents an entry in the address import table of the PE header.
  /// </summary>
  /// <remarks>
  /// For more information see http://sandsprite.com/CodeStuff/Understanding_imports.html
  /// </remarks>
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
