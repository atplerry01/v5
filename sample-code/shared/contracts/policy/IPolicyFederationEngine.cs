namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyFederationEngine
{
    Task<FederationBuildResult> BuildGraphAsync(FederationBuildInput input, CancellationToken cancellationToken = default);
    Task<FederationEvaluationResult> EvaluateAsync(FederationEvaluationInput input, CancellationToken cancellationToken = default);
}

public sealed record FederationBuildInput(
    IReadOnlyList<FederationNodeDto> Nodes,
    IReadOnlyList<FederationEdgeDto> Edges);

public sealed record FederationBuildResult(
    Guid FederationId,
    string GraphHash,
    int NodeCount,
    int EdgeCount,
    bool HasCycles,
    bool IsCompatible,
    IReadOnlyList<string> Warnings);

public sealed record FederationEvaluationInput(
    string GraphHash,
    string ActorId,
    string Action,
    string Resource,
    string? Environment,
    DateTime? Timestamp);

public sealed record FederationEvaluationResult(
    string GraphHash,
    string DecisionType,
    bool IsCompliant,
    IReadOnlyList<FederationPolicyDecision> PolicyDecisions,
    IReadOnlyList<string> DependencyChain,
    IReadOnlyList<string> CrossClusterConflicts);

public sealed record FederationPolicyDecision(
    Guid PolicyId,
    string ClusterId,
    string DecisionType,
    bool Passed);
