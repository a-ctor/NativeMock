namespace NativeMock.IntegrationTests.Infrastructure
{
  [NativeMockInterface (DeclaringTypeInheritNativeApi.DllName, DeclaringType = typeof(DeclaringTypeInheritNativeApi))]
  public interface IDeclaringTypeInheritNativeApi
  {
    string NmDeclaringTypeInherit (string value);

    [NativeMockCallback (DeclaringType = typeof(DeclaringTypeInheritNativeApi.Inner))]
    string NmDeclaringTypeOverride (string value);
  }
}
