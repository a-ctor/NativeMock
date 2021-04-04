namespace NativeMock
{
  using System;
  using System.Collections.Immutable;
  using System.Reflection;

  internal class NativeMockProxyDefinition<T>
    where T : class
  {
    private readonly Type _proxyType;
    private readonly ImmutableDictionary<MethodInfo, int> _methodHandleLookup;

    public NativeMockProxyDefinition (Type proxyType, ImmutableDictionary<MethodInfo, int> methodHandleLookup)
    {
      if (proxyType == null)
        throw new ArgumentNullException (nameof(proxyType));
      if (methodHandleLookup == null)
        throw new ArgumentNullException (nameof(methodHandleLookup));
      _proxyType = proxyType;
      _methodHandleLookup = methodHandleLookup;
    }

    public NativeMockProxy<T> CreateProxy()
    {
      var proxy = Activator.CreateInstance (_proxyType)!;
      var @object = (T) proxy;
      var controller = (INativeMockProxyController) proxy;

      return new NativeMockProxy<T> (@object, controller, _methodHandleLookup);
    }
  }
}
