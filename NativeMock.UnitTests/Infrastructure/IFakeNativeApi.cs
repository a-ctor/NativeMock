namespace NativeMock.UnitTests.Infrastructure
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
  }
}
