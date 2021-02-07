namespace NativeMock.IntegrationTests.Infrastructure
{
  [NativeMockModule (DeclaringTypeInheritNativeApi.DllName)]
  [NativeMockInterface (DeclaringType = typeof(DeclaringTypeInheritNativeApi))]
  public interface IDeclaringTypeInheritNativeApi
  {
    string NmDeclaringTypeInherit (string value);

    [NativeMockCallback (DeclaringType = typeof(DeclaringTypeInheritNativeApi.Inner))]
    string NmDeclaringTypeOverride (string value);
  }
}
