namespace NativeMock
{
  using System;

  /// <summary>
  /// Decorates a give <see cref="INativeMockInterfaceIdentifier" /> by identifying only public types.
  /// </summary>
  internal class PublicTypesOnlyNativeMockInterfaceIdentifierDecorator : INativeMockInterfaceIdentifier
  {
    private readonly INativeMockInterfaceIdentifier _innerNativeMockInterfaceIdentifier;

    public PublicTypesOnlyNativeMockInterfaceIdentifierDecorator (INativeMockInterfaceIdentifier innerNativeMockInterfaceIdentifier)
    {
      _innerNativeMockInterfaceIdentifier = innerNativeMockInterfaceIdentifier;
    }

    /// <inheritdoc />
    public bool IsNativeMockInterfaceType (Type type)
    {
      return (type.IsPublic || type.IsNestedPublic)
             && _innerNativeMockInterfaceIdentifier.IsNativeMockInterfaceType (type);
    }
  }
}
