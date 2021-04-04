namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Reflection;

  internal class NativeMockProxy<T>
    where T : class
  {
    private readonly T _object;
    private readonly INativeMockProxyController _nativeMockProxyController;
    private readonly ImmutableDictionary<MethodInfo, int> _methodHandleLookup;

    public T Object => _object;

    public NativeMockProxy (T @object, INativeMockProxyController nativeMockProxyController, ImmutableDictionary<MethodInfo, int> methodHandleLookup)
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
  }
}
