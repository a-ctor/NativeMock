namespace NativeMock.Representation
{
  using System;
  using System.Reflection;

  /// <summary>
  /// A description of a mock interface method.
  /// </summary>
  internal class NativeMockInterfaceMethodDescription : IEquatable<NativeMockInterfaceMethodDescription>
  {
    public MethodInfo InterfaceMethod { get; }

    public MethodInfo PrototypeMethod { get; }

    public NativeMockBehavior Behavior { get; }

    public NativeFunctionIdentifier Name { get; }

    public NativeMockInterfaceMethodDescription (MethodInfo interfaceMethod, MethodInfo prototypeMethod, NativeMockBehavior behavior, NativeFunctionIdentifier name)
    {
      InterfaceMethod = interfaceMethod;
      PrototypeMethod = prototypeMethod;
      Behavior = behavior;
      Name = name;
    }

    /// <inheritdoc />
    public bool Equals (NativeMockInterfaceMethodDescription? other)
    {
      if (ReferenceEquals (null, other))
        return false;
      if (ReferenceEquals (this, other))
        return true;
      return InterfaceMethod.Equals (other.InterfaceMethod) && PrototypeMethod.Equals (other.PrototypeMethod) && Behavior == other.Behavior && Name.Equals (other.Name);
    }

    /// <inheritdoc />
    public override bool Equals (object? obj)
    {
      if (ReferenceEquals (null, obj))
        return false;
      if (ReferenceEquals (this, obj))
        return true;
      if (obj.GetType() != this.GetType())
        return false;
      return Equals ((NativeMockInterfaceMethodDescription) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = InterfaceMethod.GetHashCode();
        hashCode = (hashCode * 397) ^ PrototypeMethod.GetHashCode();
        hashCode = (hashCode * 397) ^ (int) Behavior;
        hashCode = (hashCode * 397) ^ Name.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator == (NativeMockInterfaceMethodDescription? left, NativeMockInterfaceMethodDescription? right)
    {
      return Equals (left, right);
    }

    public static bool operator != (NativeMockInterfaceMethodDescription? left, NativeMockInterfaceMethodDescription? right)
    {
      return !Equals (left, right);
    }
  }
}
