using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Registry;

public interface IPolicyRegistryEngine
{
    Task<IReadOnlyList<ResolvedPolicy>> ResolvePoliciesAsync(PolicyContext context, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResolvedPolicy>> ResolveByScopeAsync(Guid scopeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResolvedPolicy>> ResolveByClassificationAsync(string classification, CancellationToken cancellationToken = default);
    Task<ResolvedPolicy?> FindByIdAsync(Guid policyId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolved policy record — decoupled from domain aggregates.
/// Contains the data needed for engine evaluation without importing domain types.
/// </summary>
public sealed record ResolvedPolicy(
    Guid PolicyId,
    string Name,
    string Domain,
    int Priority,
    Guid ScopeId,
    Guid ActiveVersionId,
    string? VersionStatus,
    bool IsLocked);

/// <summary>
/// Context for policy resolution and evaluation.
/// Engine-local type — no domain imports.
/// </summary>
public sealed record PolicyContext
{
    public required Guid ActorId { get; init; }
    public required string Action { get; init; }
    public required string Resource { get; init; }
    public required string Environment { get; init; }
    public Guid? PolicyId { get; init; }
    public DateTimeOffset Timestamp { get; init; }

    public static PolicyContext Create(
        Guid actorId, string action, string resource, string environment,
        Guid? policyId = null, DateTimeOffset? timestamp = null) =>
        new()
        {
            ActorId = actorId,
            Action = action,
            Resource = resource,
            Environment = environment,
            PolicyId = policyId,
            Timestamp = timestamp ?? default
        };
}
