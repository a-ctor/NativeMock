namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Reflection;

  internal class UnsafeMockProxy<T>
    where T : class
  {
    public T Object { get; }

    private IUnsafeMockProxy Proxy { get; }

    private ImmutableDictionary<MethodInfo, int> MethodHandleLookup { get; }

    public UnsafeMockProxy (T @object, IUnsafeMockProxy proxy, ImmutableDictionary<MethodInfo, int> methodHandleLookup)
    {
      if (@object == null)
        throw new ArgumentNullException (nameof(@object));
      if (proxy == null)
        throw new ArgumentNullException (nameof(proxy));
      if (methodHandleLookup == null)
        throw new ArgumentNullException (nameof(methodHandleLookup));

      Object = @object;
      Proxy = proxy;
      MethodHandleLookup = methodHandleLookup;
    }

    public void SetHandler (MethodInfo method, Delegate handler)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));
      if (handler == null)
        throw new ArgumentNullException (nameof(handler));
      if (!MethodHandleLookup.TryGetValue (method, out var methodHandle))
        throw new ArgumentException ("Invalid method specified.", nameof(method));

      Proxy.SetHandler (methodHandle, handler);
    }
  }
}
