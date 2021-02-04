namespace NativeMock
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Reflection.Emit;
  using System.Runtime.InteropServices;

  internal class NativeFunctionProxyFactory
  {
    // IMPORTANT: when changing the following lines the IL generation has to be fixed as well
    private static readonly Expression<Func<NativeFunctionIdentifier>> s_nativeFunctionIdentifierConstructorExpression =
      () => new NativeFunctionIdentifier ("test");

    private static readonly ConstructorInfo s_nativeFunctionIdentifierConstructorInfo =
      ((NewExpression) s_nativeFunctionIdentifierConstructorExpression.Body).Constructor!;

    private readonly MethodInfo _nativeFunctionProxyTargetMethod;

    public NativeFunctionProxyFactory (NativeFunctionHook nativeFunctionHook)
    {
      if (nativeFunctionHook == null)
        throw new ArgumentNullException (nameof(nativeFunctionHook));
      if (!nativeFunctionHook.Method.IsStatic)
        throw new ArgumentException ("The specified native call proxy must have a static target.");

      _nativeFunctionProxyTargetMethod = nativeFunctionHook.Method;
    }
    
    public NativeFunctionProxy CreateNativeFunctionProxy (NativeFunctionIdentifier name, Type nativeFunctionDelegateType)
    {
      if (name.IsInvalid)
        throw new ArgumentNullException (nameof(name));
      if (nativeFunctionDelegateType == null)
        throw new ArgumentNullException (nameof(nativeFunctionDelegateType));
      if (!nativeFunctionDelegateType.IsSubclassOf (typeof(Delegate)))
        throw new ArgumentException ("The specified native function type must be a delegate", nameof(nativeFunctionDelegateType));

      var invokeMethod = nativeFunctionDelegateType.GetMethod ("Invoke");
      var returnType = invokeMethod!.ReturnType;
      var parameters = invokeMethod.GetParameters();
      var parameterTypes = parameters.Select (e => e.ParameterType).ToArray();

      var proxyMethod = new DynamicMethod ($"{name}_NativeFunctionProxy", returnType, parameterTypes);
      var ilGenerator = proxyMethod.GetILGenerator();

      ilGenerator.Emit (OpCodes.Ldstr, name.FunctionName);
      ilGenerator.Emit (OpCodes.Newobj, s_nativeFunctionIdentifierConstructorInfo);

      // Create an array for the parameters
      ilGenerator.Emit (OpCodes.Ldc_I4, parameters.Length);
      ilGenerator.Emit (OpCodes.Newarr, typeof(object));

      // Put the function arguments into the array
      for (var i = 0; i < parameters.Length; i++)
      {
        ilGenerator.Emit (OpCodes.Dup);
        ilGenerator.Emit (OpCodes.Ldc_I4, i);
        ilGenerator.Emit (OpCodes.Ldarg, i);
        if (parameterTypes[i].IsValueType)
          ilGenerator.Emit (OpCodes.Box, parameterTypes[i]);
        ilGenerator.Emit (OpCodes.Stelem_Ref);
      }

      // Call the OnCall method
      ilGenerator.Emit (OpCodes.Call, _nativeFunctionProxyTargetMethod);

      // Convert the result into the correct type
      if (returnType == typeof(void))
      {
        ilGenerator.Emit (OpCodes.Pop);
      }
      else if (returnType.IsValueType)
      {
        ilGenerator.Emit (OpCodes.Unbox_Any, returnType);
      }
      else
      {
        ilGenerator.Emit (OpCodes.Castclass, returnType);
      }

      // Return the result
      ilGenerator.Emit (OpCodes.Ret);

      var nativeFunctionDelegate = proxyMethod.CreateDelegate (nativeFunctionDelegateType);
      var nativePtr = Marshal.GetFunctionPointerForDelegate (nativeFunctionDelegate);

      return new NativeFunctionProxy (
        name,
        nativeFunctionDelegateType,
        nativeFunctionDelegate,
        nativePtr);
    }
  }
}
