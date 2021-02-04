namespace NativeMock
{
  using System.Runtime.InteropServices;

  // see http://sandsprite.com/CodeStuff/Understanding_imports.html
  [StructLayout (LayoutKind.Sequential)]
  internal readonly struct AddressImportTableImportIdentifier
  {
    private const uint c_valueMask32 = int.MaxValue;
    private const uint c_ordinalFlag32 = ~c_valueMask32;
    private const ulong c_valueMask64 = long.MaxValue;
    private const ulong c_ordinalFlag64 = ~c_valueMask64;

    private readonly nuint _value;

    public bool IsEmpty => _value == 0;

    public unsafe bool IsOrdinal => sizeof(nint) == 4
      ? (_value & c_ordinalFlag32) != 0
      : (_value & c_ordinalFlag64) != 0;

    public unsafe nint Value => sizeof(nint) == 4 
      ? (nint) (_value & c_valueMask32)
      : (nint) (_value & c_valueMask64);
  }
}
