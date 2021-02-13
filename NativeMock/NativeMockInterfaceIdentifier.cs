namespace NativeMock
{
  using System;
  using System.Reflection;

  /// <inheritdoc />
  internal class NativeMockInterfaceIdentifier : INativeMockInterfaceIdentifier
  {
    /// <inheritdoc />
    public bool IsNativeMockInterfaceType (Type type)
    {
      return type.IsInterface
             && (!type.IsGenericType || type.IsConstructedGenericType)
             && type.GetCustomAttribute<NativeMockInterfaceAttribute>() != null;
    }
  }
}
