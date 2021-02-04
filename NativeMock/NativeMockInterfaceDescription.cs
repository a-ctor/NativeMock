namespace NativeMock
{
  using System;
  using System.Collections.Immutable;

  public record NativeMockInterfaceDescription(Type InterfaceType, ImmutableArray<NativeMockInterfaceMethodDescription> Methods);
}
