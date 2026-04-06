namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyFederationRepository
{
    Task SaveGraphAsync(FederationGraphDto graph, CancellationToken cancellationToken = default);
    Task<FederationGraphDto?> GetGraphByHashAsync(string graphHash, CancellationToken cancellationToken = default);
    Task<FederationGraphDto?> GetGraphByIdAsync(Guid federationId, CancellationToken cancellationToken = default);
    Task SaveGraphVersionAsync(FederationGraphVersionDto graph, CancellationToken cancellationToken = default);
    Task<FederationGraphVersionDto?> GetGraphVersionAsync(Guid federationId, long version, CancellationToken cancellationToken = default);
    Task SaveDiffAsync(FederationDiffDto diff, CancellationToken cancellationToken = default);
}

public sealed record FederationGraphDto(
    Guid FederationId,
    string GraphHash,
    DateTime CreatedAt,
    IReadOnlyList<FederationNodeDto> Nodes,
    IReadOnlyList<FederationEdgeDto> Edges);

public sealed record FederationNodeDto(
    Guid PolicyId,
    int Version,
    string ClusterId,
    string Status);

public sealed record FederationEdgeDto(
    Guid SourcePolicyId,
    Guid TargetPolicyId,
    string RelationType);

public sealed record FederationGraphVersionDto(
    Guid FederationId,
    string GraphHash,
    DateTime CreatedAt,
    long FederationVersion,
    string? PreviousGraphHash,
    string? CreatedByContext,
    string? ChangeReason,
    IReadOnlyList<FederationNodeDto> Nodes,
    IReadOnlyList<FederationEdgeDto> Edges);

public sealed record FederationDiffDto(
    Guid DiffId,
    Guid FederationId,
    string PreviousHash,
    string CurrentHash,
    string DiffPayload,
    DateTime CreatedAt);
