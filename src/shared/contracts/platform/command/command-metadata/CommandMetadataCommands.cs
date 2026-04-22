using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Command.CommandMetadata;

public sealed record AttachCommandMetadataCommand(
    Guid CommandMetadataId,
    Guid EnvelopeRef,
    string ActorId,
    string TraceId,
    string SpanId,
    string? PolicyId,
    string? PolicyVersion,
    int TrustScore,
    DateTimeOffset IssuedAt) : IHasAggregateId
{
    public Guid AggregateId => CommandMetadataId;
}
