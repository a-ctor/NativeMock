namespace NativeMock.IntegrationTests
{
  using System;
  using Infrastructure;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class NativeMockTests
  {
    [NativeMockInterface (FakeDllNames.Dll1)]
    public interface ITest
    {
      // ReSharper disable once UnusedMember.Global
      void NmNoop();
    }

    [Test]
    public void Constructor_TwoMocksAtTheSameTimeThrows()
    {
      var mock = new Mock<ITest>();
      using var nativeMock = new NativeMock<ITest> (mock.Object);

      Assert.That (() => new NativeMock<ITest> (mock.Object), Throws.InvalidOperationException.With.Message.StartWith ("Cannot have"));
    }

    [Test]
    public void Constructor_DisposeUnRegisteresMock()
    {
      var mock = new Mock<ITest>();
      var nativeMock = new NativeMock<ITest> (mock.Object);
      nativeMock.Dispose();

      using var nativeMock2 = new NativeMock<ITest> (mock.Object);
    }

    [Test]
    public void Constructor_LocalByDefault()
    {
      var mock = new Mock<ITest>();
      using var nativeMock = new NativeMock<ITest> (mock.Object);

      Assert.That (NativeMockRegistry.LocalSetups.TrySetup (mock.Object), Is.False);
    }

    [Test]
    public void Constructor_GlobalRegistration()
    {
      var mock = new Mock<ITest>();
      using var nativeMock = new NativeMock<ITest> (mock.Object, NativeMockScope.Global);

      Assert.That (NativeMockRegistry.GlobalSetups.TrySetup (mock.Object), Is.False);
    }

    public interface ITestApi
    {
      public delegate void RefStructParameterDelegate (Span<byte> test);

      void RefStructParameter (Span<byte> test);

      public delegate ReadOnlySpan<char> RefStructReturnDelegate();

      ReadOnlySpan<char> RefStructReturn();

      public delegate void RefParameterDelegate (ref int test);

      void RefParameter (ref int test);
    }

    [Test]
    public void NoSetupThrows()
    {
      var testUnsafeMock = new NativeMock<ITestApi>();

      Assert.That (
        () =>
        {
          Span<byte> span = stackalloc byte[4];
          testUnsafeMock.Object.RefStructParameter (span);
        },
        Throws.InvalidOperationException);
    }

    [Test]
    public void RefStructParameter()
    {
      var testUnsafeMock = new NativeMock<ITestApi>();
      testUnsafeMock.Setup<ITestApi.RefStructParameterDelegate> (
        e => e.RefStructParameter,
        e =>
        {
          Assert.That (e[0], Is.EqualTo (1));
          e[0] = 2;
        });

      Span<byte> span = stackalloc byte[4];
      span[0] = 1;
      testUnsafeMock.Object.RefStructParameter (span);

      Assert.That (span[0], Is.EqualTo (2));
    }

    private static readonly string s_refStructTest = "hello";

    [Test]
    public void RefStructReturn()
    {
      var testUnsafeMock = new NativeMock<ITestApi>();
      testUnsafeMock.Setup<ITestApi.RefStructReturnDelegate> (
        e => e.RefStructReturn,
        () => s_refStructTest.AsSpan());

      var result = testUnsafeMock.Object.RefStructReturn();

      Assert.That (result == "hello", Is.True);
    }

    [Test]
    public void RefParameter()
    {
      var testUnsafeMock = new NativeMock<ITestApi>();

      static void Handler (ref int e)
      {
        Assert.That (e, Is.EqualTo (1));
        e = 2;
      }

      testUnsafeMock.Setup<ITestApi.RefParameterDelegate> (e => e.RefParameter, Handler);

      var value = 1;
      testUnsafeMock.Object.RefParameter (ref value);

      Assert.That (value, Is.EqualTo (2));
    }
  }
}
