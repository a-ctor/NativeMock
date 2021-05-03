namespace NativeMock.Representation
{
  using System;
  using System.Collections.Immutable;
  using System.Reflection;
  using System.Runtime.InteropServices;

  /// <inheritdoc />
  internal class PInvokeMemberProvider : IPInvokeMemberProvider
  {
    /// <inheritdoc />
    public ImmutableArray<PInvokeMember> GetPInvokeMembers (Type type)
    {
      if (type == null)
        throw new ArgumentNullException (nameof(type));

      var resultBuilder = ImmutableArray.CreateBuilder<PInvokeMember>();
      foreach (var method in type.GetRuntimeMethods())
      {
        var dllImportAttribute = method.GetCustomAttribute<DllImportAttribute>();
        if (dllImportAttribute == null)
          continue;

        if ((method.Attributes & MethodAttributes.PinvokeImpl) == 0 || !method.IsStatic)
          continue;

        var nativeFunctionIdentifier = new NativeFunctionIdentifier (
          dllImportAttribute.Value,
          dllImportAttribute.EntryPoint ?? method.Name);

        resultBuilder.Add (new PInvokeMember (nativeFunctionIdentifier, method));
      }

      return resultBuilder.ToImmutable();
    }
  }
}
