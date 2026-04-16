using Whycespace.Projections.Economic.Subject.Subject.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Subject.Subject;

public sealed class SubjectProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EconomicSubjectRegisteredEventSchema>
{
    private readonly PostgresProjectionStore<EconomicSubjectReadModel> _store;

    public SubjectProjectionHandler(PostgresProjectionStore<EconomicSubjectReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            EconomicSubjectRegisteredEventSchema e => Project(
                e.AggregateId,
                s => SubjectProjectionReducer.Apply(s, e),
                "EconomicSubjectRegisteredEvent",
                envelope,
                cancellationToken),
            _ => throw new InvalidOperationException(
                $"SubjectProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(EconomicSubjectRegisteredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SubjectProjectionReducer.Apply(s, e), "EconomicSubjectRegisteredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<EconomicSubjectReadModel, EconomicSubjectReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new EconomicSubjectReadModel { SubjectId = aggregateId };
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
