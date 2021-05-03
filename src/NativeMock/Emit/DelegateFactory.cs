namespace NativeMock.Emit
{
  using System;
  using System.Collections.Concurrent;
  using System.Reflection;

  internal class DelegateFactory : IDelegateFactory
  {
    private readonly IDelegateCodeGenerator _delegateCodeGenerator;

    private readonly ConcurrentDictionary<MethodInfo, Type> _cachedDelegates = new();

    public DelegateFactory (IDelegateCodeGenerator delegateCodeGenerator)
    {
      if (delegateCodeGenerator == null)
        throw new ArgumentNullException (nameof(delegateCodeGenerator));

      _delegateCodeGenerator = delegateCodeGenerator;
    }

    /// <inheritdoc />
    public Type CreateDelegateType (MethodInfo methodInfo)
    {
      if (methodInfo == null)
        throw new ArgumentNullException (nameof(methodInfo));

      return _cachedDelegates.GetOrAdd (methodInfo, _delegateCodeGenerator.GenerateDelegateTypeForMethod);
    }
  }
}
