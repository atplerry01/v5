using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Abstractions;

namespace Whycespace.Platform.Host.Composition.Runtime.Admin;

/// <summary>
/// R4.B — registry entry that wires the admin/operator control-surface
/// services into the composition pipeline. Order sits AFTER runtime (so
/// <c>IEventFabric</c> + <c>IClock</c> + <c>IIdGenerator</c> are available)
/// and AFTER infrastructure (so the Kafka producer + DLQ store are
/// registered). Admin authorization is registered separately inside
/// <c>AddInfrastructureComposition</c> — this module owns only the admin
/// business services and the HTTP correlation accessor.
/// </summary>
public sealed class AdminCompositionModuleEntry : ICompositionModule
{
    public string Name => "admin";

    // 6 = after observability (5) — the admin services depend on the full
    // runtime + infrastructure stack being present. No other module depends
    // on admin, so placing it last is safe.
    public int Order => 6;

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAdminCompositionModule();
    }
}
