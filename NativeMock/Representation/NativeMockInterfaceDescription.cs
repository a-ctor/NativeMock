namespace NativeMock.Representation
{
  using System;
  using System.Collections.Immutable;

  /// <summary>
  /// A description of a mock interface including its target methods and module.
  /// </summary>
  internal record NativeMockInterfaceDescription(Type InterfaceType, ImmutableArray<NativeMockInterfaceMethodDescription> Methods);
}
