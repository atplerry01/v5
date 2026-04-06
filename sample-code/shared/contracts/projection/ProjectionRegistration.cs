using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Delegate for projection handlers that process events into read models.
/// </summary>
public delegate Task ProjectionHandler(ProjectionEvent @event, IProjectionStore store, CancellationToken cancellationToken);

/// <summary>
/// Registration entry for a domain projection handler.
/// </summary>
public sealed class ProjectionRegistration
{
    public required string ProjectionName { get; init; }
    public required string[] EventTypes { get; init; }
    public required ProjectionHandler Handler { get; init; }
}
