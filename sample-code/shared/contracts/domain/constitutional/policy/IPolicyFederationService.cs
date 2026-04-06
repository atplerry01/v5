namespace Whycespace.Shared.Contracts.Domain.Constitutional.Policy;

/// <summary>
/// Engine-facing federation graph contract.
/// Replaces direct import of domain FederationGraphBuilder, FederationResolver, cycle/compatibility specs.
/// </summary>
public interface IPolicyFederationGraphService
{
    FederationGraphResult BuildGraph(
        Guid federationId,
        IReadOnlyList<FederationNodeInput> nodes,
        IReadOnlyList<FederationEdgeInput> edges);

    bool HasCircularDependencies(FederationGraphResult graph);
    bool HasCompatibilityIssues(FederationGraphResult graph);
    IReadOnlyList<FederationNodeInput> ResolveEvaluationOrder(FederationGraphResult graph);
}

public sealed record FederationNodeInput(string PolicyId, string Version, string ClusterId);
public sealed record FederationEdgeInput(string SourcePolicyId, string TargetPolicyId, string RelationType);

public sealed record FederationGraphResult(
    Guid FederationId,
    string GraphHash,
    IReadOnlyList<FederationNodeInput> Nodes,
    IReadOnlyList<FederationEdgeInput> Edges);
