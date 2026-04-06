using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.GovernanceAudit;

/// <summary>
/// Governance audit log projection. Tracks all governance actions:
/// suggestion approvals, policy updates, escalations.
/// Event-driven ONLY. Idempotent, version-skip.
/// </summary>
public sealed class GovernanceAuditProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.governance-audit";

    public string[] EventTypes =>
    [
        "whyce.governance.suggestion.proposed",
        "whyce.governance.suggestion.approved",
        "whyce.governance.suggestion.rejected",
        "whyce.governance.suggestion.activated",
        "whyce.governance.audit.logged",
        "whyce.constitutional.policy.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGovernanceAuditViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();
        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);
        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var model = new GovernanceAuditReadModel
        {
            Id = $"{aggregateId}-{@event.Version}",
            Action = @event.EventType,
            ActorId = @event.Headers.TryGetValue("x-actor-id", out var a) ? a : "system",
            Timestamp = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
        _versionTracker.MarkProcessed(aggregateId, @event.Version);
    }

    public void ResetForRebuild() => _versionTracker.Reset();
}
