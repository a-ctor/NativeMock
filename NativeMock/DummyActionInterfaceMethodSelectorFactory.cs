namespace NativeMock
{
  using System;
  using System.Collections.Concurrent;

  internal class DummyActionInterfaceMethodSelectorFactory : IDummyActionInterfaceMethodSelectorFactory
  {
    private readonly IDummyActionInterfaceMethodSelectorCodeGenerator _dummyActionInterfaceMethodSelectorCodeGenerator;

    private readonly ConcurrentDictionary<Type, Type> _dummySelectors = new();

    public DummyActionInterfaceMethodSelectorFactory (IDummyActionInterfaceMethodSelectorCodeGenerator dummyActionInterfaceMethodSelectorCodeGenerator)
    {
      _dummyActionInterfaceMethodSelectorCodeGenerator = dummyActionInterfaceMethodSelectorCodeGenerator;
    }

    /// <inheritdoc />
    public DummyActionInterfaceMethodSelector<T> CreateDummyActionInterfaceMethodSelector<T>()
      where T : class
    {
      if (!typeof(T).IsInterface)
        throw new ArgumentException ("The specified type must be an interface.");

      var selectorType = _dummySelectors.GetOrAdd (typeof(T), CreateDummyActionInterfaceMethodSelectorType<T>());
      var selectorInstance = Activator.CreateInstance (selectorType)!;
      var dummyInstance = (T) selectorInstance;
      var controller = (IDummyActionInterfaceMethodSelectorController) selectorInstance;

      return new DummyActionInterfaceMethodSelector<T> (dummyInstance, controller);
    }

    private Type CreateDummyActionInterfaceMethodSelectorType<T>()
      where T : class
    {
      return _dummyActionInterfaceMethodSelectorCodeGenerator.CreateDummyActionInterfaceMethodSelector (typeof(T));
    }
  }
}
