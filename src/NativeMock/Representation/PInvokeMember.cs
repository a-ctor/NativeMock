namespace NativeMock.Representation
{
  using System;
  using System.Reflection;

  /// <summary>
  /// Represents a P/Invoke method using its native function name and its <see cref="MethodInfo" />.
  /// </summary>
  internal class PInvokeMember : IEquatable<PInvokeMember>
  {
    public NativeFunctionIdentifier Name { get; }

    public MethodInfo Method { get; }

    public PInvokeMember (NativeFunctionIdentifier name, MethodInfo method)
    {
      Name = name;
      Method = method;
    }

    /// <inheritdoc />
    public bool Equals (PInvokeMember? other)
    {
      if (ReferenceEquals (null, other))
        return false;
      if (ReferenceEquals (this, other))
        return true;
      return Name.Equals (other.Name) && Method.Equals (other.Method);
    }

    /// <inheritdoc />
    public override bool Equals (object? obj)
    {
      if (ReferenceEquals (null, obj))
        return false;
      if (ReferenceEquals (this, obj))
        return true;
      if (obj.GetType() != GetType())
        return false;
      return Equals ((PInvokeMember) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return (Name.GetHashCode() * 397) ^ Method.GetHashCode();
      }
    }

    public static bool operator == (PInvokeMember? left, PInvokeMember? right)
    {
      return Equals (left, right);
    }

    public static bool operator != (PInvokeMember? left, PInvokeMember? right)
    {
      return !Equals (left, right);
    }
  }
}
