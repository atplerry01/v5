namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Read-only event store interface for domain projections.
/// Returns ProjectionEvent (shared envelope) instead of runtime-specific types.
/// Used by projection rebuilders to replay events.
/// </summary>
public interface IProjectionEventReader
{
    Task<IReadOnlyList<ProjectionEvent>> ReadStreamAsync(string streamId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectionEvent>> ReadStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectionEvent>> ReadAllAsync(DateTimeOffset? after = null, CancellationToken cancellationToken = default);
    Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken = default);
}
