namespace NativeMock
{
  internal class HookedFunction<TDelegate>
  {
    public TDelegate Original { get; }

    public TDelegate Hook { get; }

    public HookedFunction (TDelegate original, TDelegate hook)
    {
      Original = original;
      Hook = hook;
    }
  }
}
