using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Event.EventMetadata;

public sealed record AttachEventMetadataCommand(
    Guid EventMetadataId,
    Guid EnvelopeRef,
    string ExecutionHash,
    string PolicyDecisionHash,
    string ActorId,
    string TraceId,
    string SpanId,
    DateTimeOffset IssuedAt) : IHasAggregateId
{
    public Guid AggregateId => EventMetadataId;
}
