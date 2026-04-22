using Whycespace.Projections.Shared;
using Whycespace.Projections.Trust.Identity.Profile.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Identity.Profile;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;

namespace Whycespace.Projections.Trust.Identity.Profile;

public sealed class ProfileProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProfileCreatedEventSchema>,
    IProjectionHandler<ProfileActivatedEventSchema>,
    IProjectionHandler<ProfileDeactivatedEventSchema>
{
    private readonly PostgresProjectionStore<ProfileReadModel> _store;

    public ProfileProjectionHandler(PostgresProjectionStore<ProfileReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProfileCreatedEventSchema e => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileCreatedEvent", envelope, cancellationToken),
            ProfileActivatedEventSchema e => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileActivatedEvent", envelope, cancellationToken),
            ProfileDeactivatedEventSchema e => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileDeactivatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProfileProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProfileCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileCreatedEvent", null, ct);

    public Task HandleAsync(ProfileActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileActivatedEvent", null, ct);

    public Task HandleAsync(ProfileDeactivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileDeactivatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ProfileReadModel, ProfileReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ProfileReadModel { ProfileId = aggregateId };
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
