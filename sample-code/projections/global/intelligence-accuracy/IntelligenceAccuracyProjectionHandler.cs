using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Global.IntelligenceAccuracy;

/// <summary>
/// Intelligence accuracy projection. Tracks prediction vs actual outcomes
/// and suggestion approval rates per engine. Event-driven ONLY.
/// </summary>
public sealed class IntelligenceAccuracyProjectionHandler
{
    private readonly VersionTracker _versionTracker = new();

    public string ProjectionName => "whyce.global.intelligence-accuracy";

    public string[] EventTypes =>
    [
        "whyce.intelligence.prediction.validated",
        "whyce.governance.suggestion.proposed",
        "whyce.governance.suggestion.approved",
        "whyce.governance.suggestion.rejected",
        "whyce.governance.suggestion.activated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIntelligenceAccuracyViewRepository repository, CancellationToken ct)
    {
        var aggregateId = @event.AggregateId.ToString();
        if (!_versionTracker.ShouldProcess(aggregateId, @event.Version))
            return;

        var existing = await repository.GetAsync(aggregateId, ct);
        if (existing is not null && existing.LastEventVersion >= @event.Version)
            return;

        var model = existing ?? new IntelligenceAccuracyReadModel
        {
            Id = aggregateId,
            EngineId = @event.AggregateType,
            TotalPredictions = 0,
            CorrectPredictions = 0,
            AccuracyRate = 0,
            SuggestionsProposed = 0,
            SuggestionsApproved = 0,
            SuggestionsRejected = 0,
            SuggestionsActivated = 0,
            ApprovalRate = 0,
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model with
        {
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        }, ct);

        _versionTracker.MarkProcessed(aggregateId, @event.Version);
    }

    public void ResetForRebuild() => _versionTracker.Reset();
}
