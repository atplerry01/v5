using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Governance.Cluster;

/// <summary>
/// E18.7.7 — CQRS projection for cluster governance decisions.
/// Consumes governance decision events and maintains a read model.
/// </summary>
public sealed class ClusterGovernanceProjectionHandler
{
    private readonly IProjectionStore _store;
    private readonly IClock _clock;

    public ClusterGovernanceProjectionHandler(IProjectionStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public string EventType => "GovernanceDecisionApprovedEvent";

    public async Task HandleAsync(
        JsonElement eventData,
        JsonElement metadata,
        CancellationToken cancellationToken)
    {
        var decisionId = eventData.GetProperty("DecisionId").GetGuid();
        var clusterId = eventData.GetProperty("ClusterId").GetGuid();
        var key = decisionId.ToString();

        var projection = new ClusterGovernanceReadModel
        {
            DecisionId = decisionId,
            ClusterId = clusterId,
            Status = "approved",
            LastUpdated = _clock.UtcNowOffset
        };

        await _store.SetAsync("cluster.governance", key, projection, cancellationToken);
    }
}

public sealed class ClusterGovernanceReadModel
{
    public Guid DecisionId { get; set; }
    public Guid ClusterId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset LastUpdated { get; set; }
}
