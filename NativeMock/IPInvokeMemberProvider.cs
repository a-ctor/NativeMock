namespace NativeMock
{
  using System;
  using System.Collections.Immutable;

  /// <summary>
  /// Provides methods for retrieving all PInvoke members for a specific <see cref="Type" />.
  /// </summary>
  internal interface IPInvokeMemberProvider
  {
    ImmutableArray<PInvokeMember> GetPInvokeMembers (Type type);
  }
}
