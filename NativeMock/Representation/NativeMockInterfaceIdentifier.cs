namespace NativeMock.Representation
{
  using System;
  using System.Reflection;

  /// <inheritdoc />
  internal class NativeMockInterfaceIdentifier : INativeMockInterfaceIdentifier
  {
    public static INativeMockInterfaceIdentifier Instance { get; } = new NativeMockInterfaceIdentifier();

    /// <inheritdoc />
    public bool IsNativeMockInterfaceType (Type type)
    {
      return type.IsInterface
             && (!type.IsGenericType || type.IsConstructedGenericType)
             && type.GetCustomAttribute<NativeMockInterfaceAttribute>() != null;
    }
  }
}
