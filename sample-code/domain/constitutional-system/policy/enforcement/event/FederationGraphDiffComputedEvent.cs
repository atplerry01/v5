using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record FederationGraphDiffComputedEvent(
    Guid FederationId,
    string PreviousHash,
    string CurrentHash,
    int AddedNodeCount,
    int RemovedNodeCount,
    int AddedEdgeCount,
    int RemovedEdgeCount) : DomainEvent;
