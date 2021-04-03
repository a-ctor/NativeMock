namespace NativeMock
{
  using System;
  using System.Linq.Expressions;
  using System.Reflection;

  public class UnsafeMock<T>
    where T : class
  {
    private const string c_assemblyName = "UnsafeMockProxyAssembly";
    private const string c_moduleName = "UnsafeMockProxyModule";

    private static readonly UnsafeMockProxyGenerator s_unsafeMockProxyGenerator = new (new AssemblyName (c_assemblyName), c_moduleName);
    private static readonly IUnsafeMockProxyFactory s_mockProxyFactory = new UnsafeMockProxyFactory (s_unsafeMockProxyGenerator);

    private readonly UnsafeMockProxy<T> _proxy;

    public T Object => _proxy.Object;

    public UnsafeMock()
    {
      if (!typeof(T).IsInterface)
        throw new ArgumentException ("The specified type 'T' must be an interface type.");

      _proxy = s_mockProxyFactory.CreateMockProxy<T>();
    }
    
    public void Setup<TDelegate>(Expression<Func<T, TDelegate>> selector, TDelegate handler)
      where TDelegate : Delegate
    {
      var target = GetTargetMethod (selector);
      if (target == null)
        throw new ArgumentException ("The specified selector is invalid. Please specify the interface method using an expression like 'e => e.MyInterfaceMethod'.");
      if (handler == null)
        throw new ArgumentNullException (nameof(handler));

      _proxy.SetHandler (target, handler);
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
