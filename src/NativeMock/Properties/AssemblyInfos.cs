using System.Runtime.CompilerServices;
using NativeMock;

[assembly: InternalsVisibleTo ("NativeMock.IntegrationTests")]
[assembly: InternalsVisibleTo ("NativeMock.UnitTests")]
[assembly: InternalsVisibleTo ("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo (NativeMockRegistry.ProxyAssemblyName)]
