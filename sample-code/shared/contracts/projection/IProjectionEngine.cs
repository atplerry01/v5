namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Contract for projection engines that manage registration and replay.
/// Domain projections register via this interface without coupling to runtime.
/// </summary>
public interface IProjectionEngine
{
    IReadOnlyList<ProjectionRegistration> Registrations { get; }
    void Register(ProjectionRegistration registration);
    void Register(string projectionName, string[] eventTypes, ProjectionHandler handler);
    Task ProjectAsync(CancellationToken cancellationToken = default);
    Task ProjectStreamAsync(string streamId, CancellationToken cancellationToken = default);
}
