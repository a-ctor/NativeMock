namespace NativeMock.Analyzer.Shared
{
  using Microsoft.CodeAnalysis;

  public static class MethodComparer
  {
    public static bool HaveSameSignature (IMethodSymbol left, IMethodSymbol right)
    {
      // Check the return type
      var correctReturnType = left.ReturnType.Equals (right.ReturnType)
                              && left.ReturnsByRef == right.ReturnsByRef
                              && left.ReturnsByRefReadonly == right.ReturnsByRefReadonly;

      // Check parameter count
      var correctParameterCount = left.Parameters.Length == right.Parameters.Length;
      if (!correctReturnType || !correctParameterCount)
        return false;

      for (var i = 0; i < left.Parameters.Length; i++)
      {
        var leftParameter = left.Parameters[i];
        var rightParameter = right.Parameters[i];

        var correctParameter = leftParameter.Type.Equals (rightParameter.Type)
                               && leftParameter.RefKind == rightParameter.RefKind;
        if (!correctParameter)
          return false;
      }

      return true;
    }
  }
}
