using Whycespace.Shared.Contracts.EventFabric;

namespace Whycespace.Shared.Contracts.Infrastructure.Persistence;

public interface IEventStore
{
    // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): hot-path event-store
    // contract now consumes the request/host-shutdown CancellationToken so
    // it can reach the underlying ExecuteReaderAsync / ExecuteNonQueryAsync /
    // ExecuteScalarAsync calls in PostgresEventStoreAdapter. Optional
    // default preserves source compatibility for callers that have not yet
    // been migrated.
    Task<IReadOnlyList<object>> LoadEventsAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);

    Task AppendEventsAsync(
        Guid aggregateId,
        IReadOnlyList<IEventEnvelope> envelopes,
        int expectedVersion,
        CancellationToken cancellationToken = default);
}
