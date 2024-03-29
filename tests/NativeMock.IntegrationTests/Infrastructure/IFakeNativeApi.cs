namespace NativeMock.IntegrationTests.Infrastructure
{
  using System.Runtime.InteropServices;

  [NativeMockInterface (FakeDllNames.Dll1)]
  public interface IFakeNativeApi
  {
    void NmEmpty();


    int NmStructReturn();

    string NmStringReturn();

    [return: MarshalAs (UnmanagedType.LPWStr)]
    string NmUtf8StringReturn();


    void NmStructArg (int value);

    void NmStringArg (string value);

    void NmUtf8StringArg ([MarshalAs (UnmanagedType.LPWStr)] string value);


    [NativeMockCallback ("NmRenameQ")]
    void NmRename();


    [NativeMockCallback (DeclaringType = typeof(FakeNativeApi))]
    string NmMethodDefinitionReference (string value);

    [NativeMockCallback ("NmMethodDefinitionReference2", DeclaringType = typeof(FakeNativeApi))]
    string NmMethodDefinitionReferenceRenamed (string value);

    [NativeMockCallback (DeclaringType = typeof(FakeNativeApi))]
    void NmPrivateRenamed();
  }
}
