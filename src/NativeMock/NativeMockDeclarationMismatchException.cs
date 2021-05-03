namespace NativeMock
{
  using System;
  using System.Reflection;

  /// <summary>
  /// The exception that is thrown when the declaring method does not match the interface definition.
  /// </summary>
  public class NativeMockDeclarationMismatchException : Exception
  {
    public MethodInfo InterfaceMethod { get; }

    public MethodInfo NativeDeclaration { get; }

    public NativeMockDeclarationMismatchException (MethodInfo interfaceMethod, MethodInfo nativeDeclaration, string message)
      : base ($"The specified mock interface is not compatible with its declaration: {message}")
    {
      InterfaceMethod = interfaceMethod;
      NativeDeclaration = nativeDeclaration;
    }
  }
}
