namespace NativeMock
{
  using System;

  public class NativeMockCalled
  {
    public static NativeMockCalled AtLeast (int times)
    {
      if (times < 0)
        throw new ArgumentOutOfRangeException (nameof(times));

      return new NativeMockCalled (times, int.MaxValue);
    }

    public static readonly NativeMockCalled AtLeastOnce = new (1, int.MaxValue);

    public static NativeMockCalled AtMost (int times)
    {
      if (times < 0)
        throw new ArgumentOutOfRangeException (nameof(times));

      return new NativeMockCalled (0, times);
    }

    public static readonly NativeMockCalled AtMostOnce = new (0, 1);

    public static NativeMockCalled Between (int from, int to)
    {
      if (from < 0)
        throw new ArgumentOutOfRangeException (nameof(from));
      if (to < from)
        throw new ArgumentOutOfRangeException (nameof(to));

      return new NativeMockCalled (from, to);
    }

    public static NativeMockCalled Exactly (int times)
    {
      if (times < 0)
        throw new ArgumentOutOfRangeException (nameof(times));

      return new NativeMockCalled (times, times);
    }

    public static readonly NativeMockCalled Never = new (0, 0);

    public static readonly NativeMockCalled Once = new (1, 1);

    public readonly int From;
    public readonly int To;

    private NativeMockCalled(int from, int to)
    {
      if (from < 0)
        throw new ArgumentOutOfRangeException (nameof(from));
      if (to < from)
        throw new ArgumentOutOfRangeException (nameof(to));

      From = from;
      To = to;
    }

    public bool IsValidCallCount (int times) => times >= From && times <= To;

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[{From}, {To}]";
    }
  }
}
