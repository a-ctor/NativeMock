namespace NativeMock.IntegrationTests
{
  using System;
  using System.Runtime.InteropServices;
  using Infrastructure;
  using Moq;
  using NativeApis;
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

    private class Test
    {
      [DllImport (FakeDllNames.Dll1)]
      public static extern void NmNoop();
    }

    [Test]
    public void Constructor_TwoMocksAtTheSameTimeThrows()
    {
      var mock = new Mock<ITest> (MockBehavior.Strict);
      using var nativeMock = new NativeMock<ITest> (mock.Object);

      Assert.That (() => new NativeMock<ITest> (mock.Object), Throws.InvalidOperationException.With.Message.StartWith ("Cannot have"));
    }

    [Test]
    public void Constructor_DisposeUnRegisteresMock()
    {
      var mock = new Mock<ITest> (MockBehavior.Strict);
      var nativeMock = new NativeMock<ITest> (mock.Object);
      nativeMock.Dispose();

      using var nativeMock2 = new NativeMock<ITest> (mock.Object);
    }

    [Test]
    public void Constructor_LocalByDefault()
    {
      var mock = new Mock<ITest> (MockBehavior.Strict);
      using var nativeMock = new NativeMock<ITest> (mock.Object);

      Assert.That (NativeMockRegistry.LocalSetups.TrySetup (mock.Object), Is.False);
    }

    [Test]
    public void Constructor_GlobalRegistration()
    {
      var mock = new Mock<ITest> (MockBehavior.Strict);
      using var nativeMock = new NativeMock<ITest> (mock.Object, NativeMockScope.Global);

      Assert.That (NativeMockRegistry.GlobalSetups.TrySetup (mock.Object), Is.False);
    }

    [Test]
    public void Setup_OverridesUnderlyingImplementation()
    {
      var mock = new Mock<ITest> (MockBehavior.Strict);
      using var nativeMock = new NativeMock<ITest> (mock.Object);

      bool wasCalled = false;
      nativeMock.Setup<Action> (e => e.NmNoop, () => wasCalled = true);

      Test.NmNoop();

      Assert.That (wasCalled, Is.True);
    }

    public interface ITestApi
    {
      public delegate void RefStructParameterDelegate (Span<byte> test);

      void RefStructParameter (Span<byte> test);

      public delegate ReadOnlySpan<char> RefStructReturnDelegate();

      ReadOnlySpan<char> RefStructReturn();

      public delegate void RefParameterDelegate (ref int test);

      void RefParameter (ref int test);

      public delegate ref int RefReturnDelegate();

      ref int RefReturn();
    }

    [Test]
    public void NoSetupThrowsWhenNoUnderlyingImplementation()
    {
      var testUnsafeMock = new NativeMock<ITestApi>();

      Assert.That (
        () =>
        {
          Span<byte> span = stackalloc byte[4];
          testUnsafeMock.Object.RefStructParameter (span);
        },
        Throws.TypeOf<NativeMockException>().With.Message.EqualTo ("'ITestApi.RefStructParameter' invocation failed because no setup was found."));
    }

    [Test]
    public void NoSetupUsesUnderlyingImplementationWhenAvailable()
    {
      var mock = new Mock<ITest> (MockBehavior.Strict);
      mock.Setup (e => e.NmNoop());

      using var testUnsafeMock = new NativeMock<ITest> (mock.Object);
      Test.NmNoop();

      mock.VerifyAll();
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

      Assert.That (result.SequenceEqual ("hello".AsSpan()), Is.True);
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

    private static int s_refInt;

    [Test]
    public void RefReturn()
    {
      s_refInt = 4;

      var testUnsafeMock = new NativeMock<ITestApi>();
      testUnsafeMock.SetupAlternate<ITestApi.RefReturnDelegate> (
        e => e.RefReturn(),
        () => ref s_refInt);

      ref int result = ref testUnsafeMock.Object.RefReturn();
      Assert.That (result, Is.EqualTo (4));

      result = 13;
      Assert.That (s_refInt, Is.EqualTo (13));
    }

    [Test]
    public void Forward()
    {
      var mock = new Mock<IForwardProxy.NmForwardDelegate>();
      mock.Setup (e => e (3)).Returns (5);
      TestDriver.SetForwardHandler (mock.Object);

      var testUnsafeMock = new NativeMock<IForwardProxy>();
      testUnsafeMock.SetupForward<IForwardProxy.NmForwardDelegate> (e => e.NmForward);

      Assert.That (TestDriverApi.NmForward (3), Is.EqualTo (5));

      mock.VerifyAll();
    }
  }
}
