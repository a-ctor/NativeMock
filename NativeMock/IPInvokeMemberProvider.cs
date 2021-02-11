namespace NativeMock
{
  using System;
  using System.Collections.Immutable;

  internal interface IPInvokeMemberProvider
  {
    ImmutableArray<PInvokeMember> GetPInvokeMembers (Type type);
  }
}
