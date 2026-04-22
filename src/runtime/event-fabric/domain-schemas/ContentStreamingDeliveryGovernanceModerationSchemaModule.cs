using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Moderation;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingDeliveryGovernanceModerationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("StreamFlaggedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamFlaggedEvent), typeof(StreamFlaggedEventSchema));
        sink.RegisterSchema("ModerationAssignedEvent", EventVersion.Default,
            typeof(DomainEvents.ModerationAssignedEvent), typeof(ModerationAssignedEventSchema));
        sink.RegisterSchema("ModerationDecidedEvent", EventVersion.Default,
            typeof(DomainEvents.ModerationDecidedEvent), typeof(ModerationDecidedEventSchema));
        sink.RegisterSchema("ModerationOverturnedEvent", EventVersion.Default,
            typeof(DomainEvents.ModerationOverturnedEvent), typeof(ModerationOverturnedEventSchema));

        sink.RegisterPayloadMapper("StreamFlaggedEvent", e =>
        {
            var evt = (DomainEvents.StreamFlaggedEvent)e;
            return new StreamFlaggedEventSchema(evt.ModerationId.Value, evt.TargetRef.Value, evt.FlagReason, evt.FlaggedAt.Value);
        });
        sink.RegisterPayloadMapper("ModerationAssignedEvent", e =>
        {
            var evt = (DomainEvents.ModerationAssignedEvent)e;
            return new ModerationAssignedEventSchema(evt.ModerationId.Value, evt.Moderator.Value, evt.AssignedAt.Value);
        });
        sink.RegisterPayloadMapper("ModerationDecidedEvent", e =>
        {
            var evt = (DomainEvents.ModerationDecidedEvent)e;
            return new ModerationDecidedEventSchema(evt.ModerationId.Value, evt.Decision.ToString(), evt.Rationale, evt.DecidedAt.Value);
        });
        sink.RegisterPayloadMapper("ModerationOverturnedEvent", e =>
        {
            var evt = (DomainEvents.ModerationOverturnedEvent)e;
            return new ModerationOverturnedEventSchema(evt.ModerationId.Value, evt.Rationale, evt.OverturnedAt.Value);
        });
    }
}
