namespace NativeMock.Representation
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Immutable;

  /// <summary>
  /// Decorates an inner <see cref="IPInvokeMemberProvider" /> by caching results in a thread-safe manner.
  /// </summary>
  internal class CachingPInvokeMemberProviderDecorator : IPInvokeMemberProvider
  {
    private readonly IPInvokeMemberProvider _innerPInvokeMemberProvider;

    private readonly ConcurrentDictionary<Type, ImmutableArray<PInvokeMember>> _cachedMembers = new();

    public CachingPInvokeMemberProviderDecorator (IPInvokeMemberProvider innerPInvokeMemberProvider)
    {
      if (innerPInvokeMemberProvider == null)
        throw new ArgumentNullException (nameof(innerPInvokeMemberProvider));

      _innerPInvokeMemberProvider = innerPInvokeMemberProvider;
    }

    /// <inheritdoc />
    public ImmutableArray<PInvokeMember> GetPInvokeMembers (Type type) => _cachedMembers.GetOrAdd (type, _innerPInvokeMemberProvider.GetPInvokeMembers);
  }
}
