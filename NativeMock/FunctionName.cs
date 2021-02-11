namespace NativeMock
{
  using System;

  /// <summary>
  /// Represents the name of an imported/exported function in the PE format, whose value can either be an integer or a
  /// string.
  /// </summary>
  internal readonly struct FunctionName
  {
    private readonly nint _ordinalValue;
    private readonly string? _stringValue;

    public bool IsOrdinal => _stringValue == default;

    public nint OrdinalValue => IsOrdinal ? _ordinalValue : throw new InvalidOperationException ("The function name is not an ordinal value.");

    public string StringValue => !IsOrdinal ? _stringValue! : throw new InvalidOperationException ("The function name is not a string value.");

    public FunctionName (nint ordinalValue)
    {
      if (ordinalValue < 0)
        throw new ArgumentOutOfRangeException (nameof(ordinalValue));

      _ordinalValue = ordinalValue;
      _stringValue = default;
    }

    public FunctionName (string stringValue)
    {
      _ordinalValue = default;
      _stringValue = stringValue;
    }

    public static implicit operator FunctionName (nint value) => new (value);

    public static implicit operator FunctionName (string value) => new (value);
  }
}
