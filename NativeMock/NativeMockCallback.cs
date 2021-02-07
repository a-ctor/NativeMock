namespace NativeMock
{
  using System;
  using System.Reflection;
  using System.Runtime.ExceptionServices;

  public readonly struct NativeMockCallback
  {
    private readonly object? _target;
    private readonly MethodInfo _method;

    public bool IsInvalid => _method == null;

    public NativeMockCallback (object? target, MethodInfo method)
    {
      if (method == null)
        throw new ArgumentNullException (nameof(method));

      _target = target;
      _method = method;
    }

    public object? Invoke (object?[] args)
    {
      if (args == null)
        throw new ArgumentNullException (nameof(args));
      if (IsInvalid)
        throw new InvalidOperationException ("Callback is not set up.");

      try
      {
        return _method.Invoke (_target, args);
      }
      catch (TargetInvocationException ex)
      {
        if (ex.InnerException != null)
          ExceptionDispatchInfo.Throw (ex.InnerException);
        throw;
      }
    }
  }
}
