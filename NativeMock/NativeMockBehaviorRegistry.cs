namespace NativeMock
{
  using System.Collections.Concurrent;

  /// <summary>
  /// Stores
  /// </summary>
  internal class NativeMockBehaviorRegistry
  {
    private readonly NativeMockBehavior _defaultNativeMockBehavior;

    private readonly ConcurrentDictionary<NativeFunctionIdentifier, NativeMockBehavior> _registeredNativeMockBehaviors = new();

    public NativeMockBehaviorRegistry (NativeMockBehavior defaultNativeMockBehavior)
    {
      _defaultNativeMockBehavior = defaultNativeMockBehavior;
    }

    public void SetMockBehavior (NativeFunctionIdentifier nativeFunctionIdentifier, NativeMockBehavior nativeMockBehavior)
    {
      // We do not need to track the value if it is the default value
      if (nativeMockBehavior == _defaultNativeMockBehavior)
      {
        _registeredNativeMockBehaviors.TryRemove (nativeFunctionIdentifier, out _);
      }
      else
      {
        _registeredNativeMockBehaviors[nativeFunctionIdentifier] = nativeMockBehavior;
      }
    }

    public NativeMockBehavior GetMockBehavior (NativeFunctionIdentifier nativeFunctionIdentifier)
    {
      return _registeredNativeMockBehaviors.TryGetValue (nativeFunctionIdentifier, out var mockBehavior)
        ? mockBehavior
        : _defaultNativeMockBehavior;
    }
  }
}
