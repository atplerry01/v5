namespace Whycespace.Engines.T0U.WhycePolicy.Federation;

public interface IPolicyFederationHandler
{
    Task<FederationHandlerResult> HandleAsync(
        FederationHandlerInput input,
        CancellationToken cancellationToken = default);

    Task<FederationHandlerResult> HandleIncrementalAsync(
        IncrementalFederationInput input,
        CancellationToken cancellationToken = default);
}

public sealed record FederationHandlerInput(
    IReadOnlyList<FederationNodeInput> Nodes,
    IReadOnlyList<FederationEdgeInput> Edges,
    Guid ActorId,
    string Action,
    string Resource,
    string? Environment,
    DateTimeOffset? Timestamp);

public sealed record FederationNodeInput(Guid PolicyId, int Version, string ClusterId);
public sealed record FederationEdgeInput(Guid SourcePolicyId, Guid TargetPolicyId, string RelationType);

public sealed record FederationHandlerResult(
    Guid FederationId,
    string GraphHash,
    string DecisionType,
    bool IsCompliant,
    IReadOnlyList<FederationPolicyResult> PolicyResults,
    IReadOnlyList<string> DependencyChain,
    IReadOnlyList<string> CrossClusterConflicts,
    IReadOnlyList<string> Warnings);

public sealed record FederationPolicyResult(
    Guid PolicyId,
    string ClusterId,
    string DecisionType,
    bool Passed);

public sealed record IncrementalFederationInput(
    FederationHandlerInput PreviousGraph,
    IReadOnlyList<FederationNodeInput> AddedNodes,
    IReadOnlyList<FederationNodeInput> RemovedNodes,
    IReadOnlyList<FederationEdgeInput> AddedEdges,
    IReadOnlyList<FederationEdgeInput> RemovedEdges);
