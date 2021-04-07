namespace NativeMock.Utilities
{
  using System;
  using System.Linq;

  /// <summary>
  /// Represents a hash from any hash function using its hex string representation.
  /// </summary>
  public struct GenericHash : IEquatable<GenericHash>
  {
    public readonly string Value;

    public GenericHash (string value)
    {
      if (string.IsNullOrWhiteSpace (value))
        throw new ArgumentNullException();
      if (!value.All (IsHexCharacter))
        throw new ArgumentException ("The specified value is not a valid hex string.");

      Value = value;
    }

    /// <inheritdoc />
    public bool Equals (GenericHash other) => string.Equals (Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals (object? obj) => obj is GenericHash other && Equals (other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode (Value);

    public static bool operator == (GenericHash left, GenericHash right) => left.Equals (right);

    public static bool operator != (GenericHash left, GenericHash right) => !left.Equals (right);

    public static implicit operator string (GenericHash hash) => hash.Value;

    private static bool IsHexCharacter (char character) => char.IsDigit (character) || character >= 'a' && character <= 'f' || character >= 'A' && character <= 'F';
  }
}
