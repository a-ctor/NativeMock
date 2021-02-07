namespace NativeMock.IntegrationTests.Infrastructure
{
  using System.Runtime.InteropServices;

  public interface IFakeNativeApi
  {
    void NmEmpty();


    int NmStructReturn();

    string NmStringReturn();

    [return: MarshalAs (UnmanagedType.LPUTF8Str)]
    string NmUtf8StringReturn();


    void NmStructArg (int value);

    void NmStringArg (string value);

    void NmUtf8StringArg ([MarshalAs (UnmanagedType.LPUTF8Str)] string value);


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
