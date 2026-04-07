using Whyce.Platform.Host.Composition.Operational.Sandbox.Todo;

namespace Whyce.Platform.Host.Composition;

/// <summary>
/// Static catalog of all domain bootstrap modules. The single seam where Program.cs
/// meets per-domain wiring — Program.cs imports only this catalog and never references
/// individual domain modules by name.
///
/// To onboard a new domain: add one entry to <see cref="All"/>.
/// </summary>
public static class BootstrapModuleCatalog
{
    public static IReadOnlyList<IDomainBootstrapModule> All { get; } =
    [
        new TodoBootstrap()
    ];
}
