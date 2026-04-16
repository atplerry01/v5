namespace Whycespace.Shared.Contracts.Infrastructure.Messaging;

public interface IOutbox
{
    // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): hot-path outbox
    // contract now consumes the request/host-shutdown CancellationToken
    // so it can reach the underlying ExecuteNonQueryAsync calls in
    // PostgresOutboxAdapter. Optional default preserves source
    // compatibility for callers that have not yet been migrated.
    // K-AGGREGATE-ID-HEADER-01 / D9: aggregateId is the authoritative source
    // for the outbox row's aggregate_id column AND the downstream Kafka
    // `aggregate-id` header. Callers MUST pass the value held by the
    // EventEnvelope they constructed — never rely on reflection over the
    // domain event payload, which is brittle (new aggregates require allowlist
    // updates) and silently emits Guid.Empty for unrecognised events.
    Task EnqueueAsync(
        Guid correlationId,
        Guid aggregateId,
        IReadOnlyList<object> events,
        string topic,
        CancellationToken cancellationToken = default);
}
