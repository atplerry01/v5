using Whycespace.Projections.Business.Customer.IdentityAndProfile.Profile.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Customer.IdentityAndProfile.Profile;

public sealed class ProfileProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProfileCreatedEventSchema>,
    IProjectionHandler<ProfileRenamedEventSchema>,
    IProjectionHandler<ProfileDescriptorSetEventSchema>,
    IProjectionHandler<ProfileDescriptorRemovedEventSchema>,
    IProjectionHandler<ProfileActivatedEventSchema>,
    IProjectionHandler<ProfileArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ProfileReadModel> _store;

    public ProfileProjectionHandler(PostgresProjectionStore<ProfileReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ProfileCreatedEventSchema e            => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileCreatedEvent",            envelope, cancellationToken),
            ProfileRenamedEventSchema e            => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileRenamedEvent",            envelope, cancellationToken),
            ProfileDescriptorSetEventSchema e      => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileDescriptorSetEvent",      envelope, cancellationToken),
            ProfileDescriptorRemovedEventSchema e  => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileDescriptorRemovedEvent",  envelope, cancellationToken),
            ProfileActivatedEventSchema e          => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileActivatedEvent",          envelope, cancellationToken),
            ProfileArchivedEventSchema e           => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileArchivedEvent",           envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProfileProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ProfileCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileCreatedEvent", null, ct);

    public Task HandleAsync(ProfileRenamedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileRenamedEvent", null, ct);

    public Task HandleAsync(ProfileDescriptorSetEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileDescriptorSetEvent", null, ct);

    public Task HandleAsync(ProfileDescriptorRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileDescriptorRemovedEvent", null, ct);

    public Task HandleAsync(ProfileActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileActivatedEvent", null, ct);

    public Task HandleAsync(ProfileArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ProfileProjectionReducer.Apply(s, e), "ProfileArchivedEvent", null, ct);

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
