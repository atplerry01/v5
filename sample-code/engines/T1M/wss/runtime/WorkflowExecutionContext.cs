using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T1M.Wss.Execution;

/// <summary>
/// Immutable execution context carried through the workflow execution pipeline.
/// Contains correlation, routing, and metadata — no domain state.
/// </summary>
public sealed record WorkflowExecutionContext(
    string WorkflowId,
    string Cluster,
    string Subcluster,
    string Domain,
    string Context,
    Guid CorrelationId,
    DateTimeOffset Timestamp)
{
    public static WorkflowExecutionContext Create(
        string workflowId,
        string cluster,
        string subcluster,
        string domain,
        string context,
        IClock clock,
        IIdGenerator idGen)
    {
        return new WorkflowExecutionContext(
            workflowId,
            cluster,
            subcluster,
            domain,
            context,
            idGen.DeterministicGuid($"WorkflowExecution:{workflowId}:{cluster}:{subcluster}:{domain}:{context}"),
            clock.UtcNowOffset);
    }
}
