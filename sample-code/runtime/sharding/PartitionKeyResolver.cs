using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Sharding;

/// <summary>
/// Resolves the partition key for a command envelope.
/// Same key → same Kafka partition → ordering guaranteed per aggregate.
/// </summary>
public interface IPartitionKeyResolver
{
    string Resolve(CommandEnvelope envelope);
}

public sealed class PartitionKeyResolver : IPartitionKeyResolver
{
    /// <summary>
    /// Resolves partition key with priority:
    ///   1. Explicit AggregateId from headers
    ///   2. CorrelationId (groups related commands)
    ///   3. CommandId fallback (unique distribution)
    /// </summary>
    /// <summary>
    /// Resolves partition key with priority:
    ///   1. First-class AggregateId on envelope
    ///   2. Explicit AggregateId from headers
    ///   3. CorrelationId (groups related commands)
    /// </summary>
    public string Resolve(CommandEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (!string.IsNullOrWhiteSpace(envelope.AggregateId))
        {
            return envelope.AggregateId;
        }

        if (envelope.Metadata.Headers.TryGetValue(HeaderKeys.AggregateId, out var aggregateId)
            && !string.IsNullOrWhiteSpace(aggregateId))
        {
            return aggregateId;
        }

        return envelope.CorrelationId;
    }

    public static class HeaderKeys
    {
        public const string AggregateId = "x-aggregate-id";
        public const string WorkflowId = "x-workflow-id";
    }
}
