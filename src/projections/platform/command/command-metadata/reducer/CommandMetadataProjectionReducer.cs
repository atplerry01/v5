using Whycespace.Shared.Contracts.Events.Platform.Command.CommandMetadata;
using Whycespace.Shared.Contracts.Platform.Command.CommandMetadata;

namespace Whycespace.Projections.Platform.Command.CommandMetadata.Reducer;

public static class CommandMetadataProjectionReducer
{
    public static CommandMetadataReadModel Apply(CommandMetadataReadModel state, CommandMetadataAttachedEventSchema e, DateTimeOffset at) =>
        state with
        {
            CommandMetadataId = e.AggregateId,
            EnvelopeRef = e.EnvelopeRef,
            ActorId = e.ActorId,
            TraceId = e.TraceId,
            SpanId = e.SpanId,
            PolicyId = e.PolicyId,
            PolicyVersion = e.PolicyVersion,
            TrustScore = e.TrustScore,
            IssuedAt = e.IssuedAt,
            LastModifiedAt = at
        };
}
