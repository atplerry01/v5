using Whycespace.Projections.Business.Customer.IdentityAndProfile.Account.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Customer.IdentityAndProfile.Account;

public sealed class AccountProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AccountCreatedEventSchema>,
    IProjectionHandler<AccountRenamedEventSchema>,
    IProjectionHandler<AccountActivatedEventSchema>,
    IProjectionHandler<AccountSuspendedEventSchema>,
    IProjectionHandler<AccountClosedEventSchema>
{
    private readonly PostgresProjectionStore<AccountReadModel> _store;

    public AccountProjectionHandler(PostgresProjectionStore<AccountReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AccountCreatedEventSchema e   => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountCreatedEvent",   envelope, cancellationToken),
            AccountRenamedEventSchema e   => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountRenamedEvent",   envelope, cancellationToken),
            AccountActivatedEventSchema e => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountActivatedEvent", envelope, cancellationToken),
            AccountSuspendedEventSchema e => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountSuspendedEvent", envelope, cancellationToken),
            AccountClosedEventSchema e    => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountClosedEvent",    envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AccountProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AccountCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountCreatedEvent", null, ct);

    public Task HandleAsync(AccountRenamedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountRenamedEvent", null, ct);

    public Task HandleAsync(AccountActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountActivatedEvent", null, ct);

    public Task HandleAsync(AccountSuspendedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountSuspendedEvent", null, ct);

    public Task HandleAsync(AccountClosedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccountProjectionReducer.Apply(s, e), "AccountClosedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AccountReadModel, AccountReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AccountReadModel { AccountId = aggregateId };
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
