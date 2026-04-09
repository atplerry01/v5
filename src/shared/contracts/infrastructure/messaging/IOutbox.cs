namespace Whyce.Shared.Contracts.Infrastructure.Messaging;

public interface IOutbox
{
    // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): hot-path outbox
    // contract now consumes the request/host-shutdown CancellationToken
    // so it can reach the underlying ExecuteNonQueryAsync calls in
    // PostgresOutboxAdapter. Optional default preserves source
    // compatibility for callers that have not yet been migrated.
    Task EnqueueAsync(
        Guid correlationId,
        IReadOnlyList<object> events,
        string topic,
        CancellationToken cancellationToken = default);
}
