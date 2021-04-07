namespace NativeMock.Registration
{
  internal class NativeMockSetupInternalRegistryFactory : INativeMockSetupInternalRegistryFactory
  {
    /// <inheritdoc />
    public INativeMockSetupInternalRegistry CreateMockSetupRegistry() => new NativeMockSetupRegistry();
  }
}
