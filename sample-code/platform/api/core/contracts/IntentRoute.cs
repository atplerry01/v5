using Whycespace.Platform.Api.Core.Contracts.Context;

namespace Whycespace.Platform.Api.Core.Contracts;

/// <summary>
/// Fully resolved route for a classified intent.
/// Maps ClassifiedIntent → Cluster → Authority → SubCluster → WorkflowKey → WSS.
///
/// Cluster names follow the Whycespace taxonomy: "Whyce" + ClusterName.
/// ExecutionTarget is always "wss" — platform never calls engines directly.
/// Tenant and Region are propagated for multi-tenant/multi-region routing.
/// </summary>
public sealed record IntentRoute
{
    public required string Cluster { get; init; }
    public required string Authority { get; init; }
    public required string SubCluster { get; init; }
    public required string WorkflowKey { get; init; }
    public required string ExecutionTarget { get; init; }
    public required string Domain { get; init; }
    public required string CommandType { get; init; }
    public TenantContext? Tenant { get; init; }
    public RegionContext? Region { get; init; }
}
