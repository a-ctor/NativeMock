namespace NativeMock.Emit
{
  using System;
  using System.Linq.Expressions;
  using System.Reflection;

  public static class ReflectionInfoUtility
  {
    public static ConstructorInfo SelectConstructor<T> (Expression<Func<T>> expression)
    {
      if (expression == null)
        throw new ArgumentNullException (nameof(expression));

      var newExpression = (NewExpression) expression.Body;
      if (newExpression == null)
        throw new ArgumentException ("The specified constructor selector is invalid.");

      return newExpression.Constructor!;
    }

    public static MethodInfo SelectMethod (Expression<Action> expression)
    {
      if (expression == null)
        throw new ArgumentNullException (nameof(expression));

      var methodCallExpression = (MethodCallExpression) expression.Body;
      if (methodCallExpression == null)
        throw new ArgumentException ("The specified static method selector is invalid.");

      return methodCallExpression.Method;
    }

    public static MethodInfo SelectMethod<T> (Expression<Action<T>> expression)
    {
      if (expression == null)
        throw new ArgumentNullException (nameof(expression));

      var methodCallExpression = (MethodCallExpression) expression.Body;
      if (methodCallExpression == null)
        throw new ArgumentException ("The specified instance method selector is invalid.");

      return methodCallExpression.Method;
    }

    public static MethodInfo SelectGetter<T> (Expression<Func<T, object?>> expression)
    {
      if (expression == null)
        throw new ArgumentNullException (nameof(expression));

      var memberExpression = (MemberExpression) expression.Body;
      if (memberExpression == null)
        throw new ArgumentException ("The specified instance property selector is invalid.");

      var propertyInfo = memberExpression.Member as PropertyInfo;
      if (propertyInfo == null)
        throw new ArgumentException ("The specified selector does not select a property.");

      var getMethod = propertyInfo.GetMethod;
      if (getMethod == null)
        throw new ArgumentException ("The selected property does not have a getter.");

      return getMethod;
    }

    public static MethodInfo GetEqualityOperatorMethod (Type type, Type left, Type right)
    {
      var equalityOperatorMethod = type.GetMethod ("op_Equality", new[] {left, right});
      if (equalityOperatorMethod == null)
        throw new ArgumentException ($"No equality operator was found for type '{type}' matching '{left} == {right}'.");

      return equalityOperatorMethod;
    }
  }
}
