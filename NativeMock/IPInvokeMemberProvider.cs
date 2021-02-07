namespace NativeMock
{
  using System;
  using System.Collections.Immutable;

  public interface IPInvokeMemberProvider
  {
    ImmutableArray<PInvokeMember> GetPInvokeMembers (Type type);
  }
}
