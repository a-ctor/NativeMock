namespace NativeMock.Emit
{
  using System;
  using System.Collections.Concurrent;
  using System.Reflection;

  public class NativeFunctionForwardProxyFactory : INativeFunctionForwardProxyFactory
  {
    private readonly INativeFunctionForwardProxyCodeGenerator _nativeFunctionForwardProxyCodeGenerator;

    private readonly ConcurrentDictionary<MethodInfo, Delegate> _nativeForwardProxyFunctionsCache = new();

    public NativeFunctionForwardProxyFactory (INativeFunctionForwardProxyCodeGenerator nativeFunctionForwardProxyCodeGenerator)
    {
      if (nativeFunctionForwardProxyCodeGenerator == null)
        throw new ArgumentNullException (nameof(nativeFunctionForwardProxyCodeGenerator));

      _nativeFunctionForwardProxyCodeGenerator = nativeFunctionForwardProxyCodeGenerator;
    }

    /// <inheritdoc />
    public Delegate CreateNativeFunctionForwardProxy (MethodInfo method)
    {
      return _nativeForwardProxyFunctionsCache.GetOrAdd (method, _nativeFunctionForwardProxyCodeGenerator.GenerateNativeFunctionForwardProxy);
    }
  }
}
