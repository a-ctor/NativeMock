namespace NativeMock.Emit
{
  using System;
  using System.Collections.Immutable;
  using System.Reflection;

  internal class NativeMockProxy<T>
    where T : class
  {
    private readonly T _object;
    private readonly INativeMockProxyController<T> _nativeMockProxyController;
    private readonly ImmutableDictionary<MethodInfo, int> _methodHandleLookup;

    public T Object => _object;

    public T? UnderlyingImplementation
    {
      set { _nativeMockProxyController.SetUnderlyingImplementation (value); }
    }

    public NativeMockProxy (T @object, INativeMockProxyController<T> nativeMockProxyController, ImmutableDictionary<MethodInfo, int> methodHandleLookup)
    {
      if (@object == null)
        throw new ArgumentNullException (nameof(@object));
      if (nativeMockProxyController == null)
        throw new ArgumentNullException (nameof(nativeMockProxyController));
      if (methodHandleLookup == null)
        throw new ArgumentNullException (nameof(methodHandleLookup));

      _object = @object;
      _nativeMockProxyController = nativeMockProxyController;
      _methodHandleLookup = methodHandleLookup;
    }

    public void Verify()
    {
      foreach (var method in _methodHandleLookup.Keys)
        Verify (method, NativeMockCalled.AtLeastOnce);
    }

    public void Verify (MethodInfo method, NativeMockCalled called)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));
      if (called == null)
        throw new ArgumentNullException (nameof(called));
      if (!_methodHandleLookup.TryGetValue (method, out var methodHandle))
        throw new ArgumentException ("Invalid method specified.", nameof(method));

      if (_nativeMockProxyController.GetMethodHandler (methodHandle) == null)
        return;

      var setupCalled = _nativeMockProxyController.GetMethodHandlerCallCount (methodHandle);
      if (!called.IsValidCallCount (setupCalled))
        throw new NativeMockException ($"This mock failed verification because the setup for '{method.Name}' was not matched.");
    }

    public void SetMethodHandler (MethodInfo method, Delegate handler)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));
      if (handler == null)
        throw new ArgumentNullException (nameof(handler));
      if (!_methodHandleLookup.TryGetValue (method, out var methodHandle))
        throw new ArgumentException ("Invalid method specified.", nameof(method));

      _nativeMockProxyController.SetMethodHandler (methodHandle, handler);
    }

    public void Reset()
    {
      _nativeMockProxyController.Reset();
    }
  }
}
