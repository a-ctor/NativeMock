namespace NativeMock.Hooking
{
  /// <summary>
  /// Represents a hooked function using the original and the hooked <typeparamref name="TDelegate" />.
  /// </summary>
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
