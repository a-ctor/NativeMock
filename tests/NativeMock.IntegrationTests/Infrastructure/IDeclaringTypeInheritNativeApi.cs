namespace NativeMock.IntegrationTests.Infrastructure
{
  [NativeMockInterface (FakeDllNames.Dll1, DeclaringType = typeof(DeclaringTypeInheritNativeApi))]
  public interface IDeclaringTypeInheritNativeApi
  {
    string NmDeclaringTypeInherit (string value);

    [NativeMockCallback (DeclaringType = typeof(DeclaringTypeInheritNativeApi.Inner))]
    string NmDeclaringTypeOverride (string value);
  }
}
