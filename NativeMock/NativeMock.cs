namespace NativeMock
{
  using System;
  using System.Linq.Expressions;
  using System.Reflection;

  public class NativeMock<T> : IDisposable
    where T : class
  {
    private readonly INativeMockSetupInternalRegistry _setupRegistry;
    private readonly NativeMockProxy<T>? _proxy;

    private bool _disposed;

    public T Object { get; }

    public NativeMock (NativeMockScope scope = NativeMockScope.Default)
    {
      if (!typeof(T).IsInterface)
        throw new ArgumentException ("The specified T type parameter must be an interface");

      // todo register interface in addition to mocking

      _setupRegistry = NativeMockRegistry.GetSetupRegistryForScope (scope);
      _proxy = NativeMockRegistry.CreateProxy<T>();

      Object = _proxy.Object;
    }

    public NativeMock (T implementation, NativeMockScope scope = NativeMockScope.Default)
    {
      if (implementation == null)
        throw new ArgumentNullException (nameof(implementation));
      if (!typeof(T).IsInterface)
        throw new ArgumentException ("The specified T type parameter must be an interface");
      if (!NativeMockRegistry.IsRegistered<T>())
        throw new InvalidOperationException ($"The specified type '{typeof(T)}' is not registered as a native mock interface. Use NativeMockRegistry.Register as early in the program as possible to register an interface.");

      var setupRegistry = NativeMockRegistry.GetSetupRegistryForScope (scope);
      if (!setupRegistry.TrySetup (implementation))
        throw new InvalidOperationException ("Cannot have two native mocks of the same interface in the same context at the same time.");

      // todo allow mocking native mock interfaces

      _setupRegistry = setupRegistry;
      _proxy = null;

      Object = implementation;
    }

    /// <inheritdoc />
    public void Dispose()
    {
      if (_disposed)
        return;

      _setupRegistry.Reset<T>();
      _disposed = true;
    }

    public void Setup<TDelegate> (Expression<Func<T, TDelegate>> selector, TDelegate handler)
      where TDelegate : Delegate
    {
      var target = GetTargetMethod (selector);
      if (target == null)
        throw new ArgumentException ("The specified selector is invalid. Please specify the interface method using an expression like 'e => e.MyInterfaceMethod'.");
      if (handler == null)
        throw new ArgumentNullException (nameof(handler));

      if (_proxy == null)
        throw new InvalidOperationException ("Setups are only allow with a parameterless constructor.");

      _proxy.SetMethodHandler (target, handler);
    }

    private MethodInfo? GetTargetMethod<TDelegate> (Expression<Func<T, TDelegate>> selector)
    {
      if (selector.Body is not UnaryExpression unaryExpression || selector.Body.NodeType != ExpressionType.Convert)
        return null;

      if (unaryExpression.Operand is not MethodCallExpression methodCallExpression)
        return null;

      if (methodCallExpression.Object is not ConstantExpression constantExpression || constantExpression.Type != typeof(MethodInfo))
        return null;

      var methodInfo = (MethodInfo?) constantExpression.Value;
      if (methodInfo == null || methodInfo.DeclaringType != typeof(T))
        return null;

      return methodInfo;
    }
  }
}
