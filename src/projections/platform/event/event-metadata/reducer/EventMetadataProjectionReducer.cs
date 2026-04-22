using Whycespace.Shared.Contracts.Events.Platform.Event.EventMetadata;
using Whycespace.Shared.Contracts.Platform.Event.EventMetadata;

namespace Whycespace.Projections.Platform.Event.EventMetadata.Reducer;

public static class EventMetadataProjectionReducer
{
    public static EventMetadataReadModel Apply(EventMetadataReadModel state, EventMetadataAttachedEventSchema e, DateTimeOffset at) =>
        state with
        {
            EventMetadataId = e.AggregateId,
            EnvelopeRef = e.EnvelopeRef,
            ExecutionHash = e.ExecutionHash,
            PolicyDecisionHash = e.PolicyDecisionHash,
            ActorId = e.ActorId,
            TraceId = e.TraceId,
            SpanId = e.SpanId,
            IssuedAt = e.IssuedAt,
            LastModifiedAt = at
        };
}
