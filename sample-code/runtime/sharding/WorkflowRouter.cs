using System.Security.Cryptography;
using System.Text;
using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Sharding;

/// <summary>
/// Routes workflows to consistent nodes based on affinity key.
/// Same workflow always routes to the same node for ordering guarantees.
/// </summary>
public interface IWorkflowRouter
{
    /// <summary>
    /// Returns the target node index for the given envelope.
    /// </summary>
    int ResolveNode(CommandEnvelope envelope, int nodeCount);

    /// <summary>
    /// Returns the partition key used for affinity routing.
    /// </summary>
    string ResolveAffinityKey(CommandEnvelope envelope);
}

public sealed class WorkflowRouter : IWorkflowRouter
{
    private readonly IPartitionKeyResolver _partitionKeyResolver;

    public WorkflowRouter(IPartitionKeyResolver partitionKeyResolver)
    {
        ArgumentNullException.ThrowIfNull(partitionKeyResolver);
        _partitionKeyResolver = partitionKeyResolver;
    }

    public string ResolveAffinityKey(CommandEnvelope envelope)
    {
        // Workflow affinity is determined by the same key used for Kafka partitioning.
        // If a workflow-specific key is present, prefer it.
        if (envelope.Metadata.Headers.TryGetValue(PartitionKeyResolver.HeaderKeys.WorkflowId, out var workflowId)
            && !string.IsNullOrWhiteSpace(workflowId))
        {
            return workflowId;
        }

        return _partitionKeyResolver.Resolve(envelope);
    }

    public int ResolveNode(CommandEnvelope envelope, int nodeCount)
    {
        if (nodeCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(nodeCount), "Node count must be positive.");

        var key = ResolveAffinityKey(envelope);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

        // Use first 4 bytes of hash as a consistent integer for modulo routing.
        var value = BitConverter.ToUInt32(hash, 0);
        return (int)(value % (uint)nodeCount);
    }
}
