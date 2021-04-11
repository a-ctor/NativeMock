namespace NativeMock.Emit
{
  using System;
  using System.Collections.Concurrent;
  using System.Reflection;

  internal class CachingDelegateCodeGeneratorDecorator : IDelegateCodeGenerator
  {
    private readonly IDelegateCodeGenerator _delegateCodeGeneratorImplementation;

    private readonly ConcurrentDictionary<MethodInfo, Type> _cachedDelegates = new();

    public CachingDelegateCodeGeneratorDecorator (IDelegateCodeGenerator delegateCodeGeneratorImplementation)
    {
      if (delegateCodeGeneratorImplementation == null)
        throw new ArgumentNullException (nameof(delegateCodeGeneratorImplementation));

      _delegateCodeGeneratorImplementation = delegateCodeGeneratorImplementation;
    }

    /// <inheritdoc />
    public Type CreateDelegateType (MethodInfo methodInfo)
    {
      if (methodInfo == null)
        throw new ArgumentNullException (nameof(methodInfo));

      return _cachedDelegates.GetOrAdd (methodInfo, _delegateCodeGeneratorImplementation.CreateDelegateType);
    }
  }
}
