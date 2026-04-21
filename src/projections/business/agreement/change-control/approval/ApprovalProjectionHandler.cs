using Whycespace.Projections.Business.Agreement.ChangeControl.Approval.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Approval;

public sealed class ApprovalProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ApprovalCreatedEventSchema>,
    IProjectionHandler<ApprovalApprovedEventSchema>,
    IProjectionHandler<ApprovalRejectedEventSchema>
{
    private readonly PostgresProjectionStore<ApprovalReadModel> _store;

    public ApprovalProjectionHandler(PostgresProjectionStore<ApprovalReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ApprovalCreatedEventSchema e  => Project(e.AggregateId, s => ApprovalProjectionReducer.Apply(s, e), "ApprovalCreatedEvent",  envelope, cancellationToken),
            ApprovalApprovedEventSchema e => Project(e.AggregateId, s => ApprovalProjectionReducer.Apply(s, e), "ApprovalApprovedEvent", envelope, cancellationToken),
            ApprovalRejectedEventSchema e => Project(e.AggregateId, s => ApprovalProjectionReducer.Apply(s, e), "ApprovalRejectedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ApprovalProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ApprovalCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ApprovalProjectionReducer.Apply(s, e), "ApprovalCreatedEvent", null, ct);

    public Task HandleAsync(ApprovalApprovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ApprovalProjectionReducer.Apply(s, e), "ApprovalApprovedEvent", null, ct);

    public Task HandleAsync(ApprovalRejectedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ApprovalProjectionReducer.Apply(s, e), "ApprovalRejectedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ApprovalReadModel, ApprovalReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ApprovalReadModel { ApprovalId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
