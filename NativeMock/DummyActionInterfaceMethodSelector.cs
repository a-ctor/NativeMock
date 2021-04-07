namespace NativeMock
{
  using System;
  using System.Reflection;

  internal class DummyActionInterfaceMethodSelector<T>
    where T : class
  {
    private readonly T _dummyInstance;
    private readonly IDummyActionInterfaceMethodSelectorController _controller;

    public DummyActionInterfaceMethodSelector (T dummyInstance, IDummyActionInterfaceMethodSelectorController controller)
    {
      if (dummyInstance == null)
        throw new ArgumentNullException (nameof(dummyInstance));
      if (controller == null)
        throw new ArgumentNullException (nameof(controller));

      _dummyInstance = dummyInstance;
      _controller = controller;
    }

    public MethodInfo GetSelectedMethod (Action<T> selector)
    {
      if (selector == null)
        throw new ArgumentNullException (nameof(selector));

      _controller.Reset();

      try
      {
        selector (_dummyInstance);
      }
      catch
      {
        // We can ignore any exception here because either an invalid selector was provided by the user
        // or the dummy method threw an exception because it cannot return. Regardless we can just check
        // the controllers for the set count and the resulting method
      }

      var setCount = _controller.GetSetCount();
      var result = _controller.GetResult();
      if (setCount > 1)
        throw new InvalidOperationException ("The specified interface method selector called multiple interface methods.");
      if (setCount == 0 || result == null)
        throw new InvalidOperationException ("The specified interface method selector did not call an interface method.");
      
      return result;
    }
  }
}
