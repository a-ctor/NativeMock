namespace NativeMock
{
  using System;

  /// <summary>
  /// Represents a proxy method that was generated as a replacement for a native method.
  /// </summary>
  internal class NativeFunctionProxy
  {
    public NativeFunctionIdentifier Name { get; }

    public Type DelegateType { get; }

    public Delegate Delegate { get; }

    public IntPtr NativePtr { get; }

    public NativeFunctionProxy (NativeFunctionIdentifier name, Type delegateType, Delegate @delegate, IntPtr nativePtr)
    {
      if (name.IsInvalid)
        throw new ArgumentNullException (nameof(name));
      if (delegateType == null)
        throw new ArgumentNullException (nameof(delegateType));
      if (@delegate == null)
        throw new ArgumentNullException (nameof(@delegate));
      if (!delegateType.IsAssignableTo (typeof(Delegate)))
        throw new ArgumentException ("Must be a delegate type.", nameof(delegateType));
      if (!@delegate.GetType().IsAssignableTo (delegateType))
        throw new ArgumentException ("Must be assignable to the specified delegate type.");

      Name = name;
      DelegateType = delegateType;
      Delegate = @delegate;
      NativePtr = nativePtr;
    }
  }
}
