namespace NativeMock.Representation
{
  using System;
  using System.Collections.Immutable;

  /// <summary>
  /// A description of a mock interface including its target methods and module.
  /// </summary>
  internal class NativeMockInterfaceDescription : IEquatable<NativeMockInterfaceDescription>
  {
    public Type InterfaceType { get; }

    public ImmutableArray<NativeMockInterfaceMethodDescription> Methods { get; }

    public NativeMockInterfaceDescription (Type interfaceType, ImmutableArray<NativeMockInterfaceMethodDescription> methods)
    {
      InterfaceType = interfaceType;
      Methods = methods;
    }

    /// <inheritdoc />
    public bool Equals (NativeMockInterfaceDescription? other)
    {
      if (ReferenceEquals (null, other))
        return false;
      if (ReferenceEquals (this, other))
        return true;
      return InterfaceType == other.InterfaceType && Methods.Equals (other.Methods);
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
      return Equals ((NativeMockInterfaceDescription) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return (InterfaceType.GetHashCode() * 397) ^ Methods.GetHashCode();
      }
    }

    public static bool operator == (NativeMockInterfaceDescription? left, NativeMockInterfaceDescription? right)
    {
      return Equals (left, right);
    }

    public static bool operator != (NativeMockInterfaceDescription? left, NativeMockInterfaceDescription? right)
    {
      return !Equals (left, right);
    }
  }
}
