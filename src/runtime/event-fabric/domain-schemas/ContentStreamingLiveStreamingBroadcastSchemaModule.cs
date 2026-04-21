using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Broadcast;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingLiveStreamingBroadcastSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("BroadcastCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.BroadcastCreatedEvent), typeof(BroadcastCreatedEventSchema));
        sink.RegisterSchema("BroadcastScheduledEvent", EventVersion.Default,
            typeof(DomainEvents.BroadcastScheduledEvent), typeof(BroadcastScheduledEventSchema));
        sink.RegisterSchema("BroadcastStartedEvent", EventVersion.Default,
            typeof(DomainEvents.BroadcastStartedEvent), typeof(BroadcastStartedEventSchema));
        sink.RegisterSchema("BroadcastPausedEvent", EventVersion.Default,
            typeof(DomainEvents.BroadcastPausedEvent), typeof(BroadcastPausedEventSchema));
        sink.RegisterSchema("BroadcastResumedEvent", EventVersion.Default,
            typeof(DomainEvents.BroadcastResumedEvent), typeof(BroadcastResumedEventSchema));
        sink.RegisterSchema("BroadcastEndedEvent", EventVersion.Default,
            typeof(DomainEvents.BroadcastEndedEvent), typeof(BroadcastEndedEventSchema));
        sink.RegisterSchema("BroadcastCancelledEvent", EventVersion.Default,
            typeof(DomainEvents.BroadcastCancelledEvent), typeof(BroadcastCancelledEventSchema));

        sink.RegisterPayloadMapper("BroadcastCreatedEvent", e =>
        {
            var evt = (DomainEvents.BroadcastCreatedEvent)e;
            return new BroadcastCreatedEventSchema(evt.BroadcastId.Value, evt.StreamRef.Value, evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("BroadcastScheduledEvent", e =>
        {
            var evt = (DomainEvents.BroadcastScheduledEvent)e;
            return new BroadcastScheduledEventSchema(evt.BroadcastId.Value, evt.Window.ScheduledStart.Value, evt.Window.ScheduledEnd.Value, evt.ScheduledAt.Value);
        });
        sink.RegisterPayloadMapper("BroadcastStartedEvent", e =>
        {
            var evt = (DomainEvents.BroadcastStartedEvent)e;
            return new BroadcastStartedEventSchema(evt.BroadcastId.Value, evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("BroadcastPausedEvent", e =>
        {
            var evt = (DomainEvents.BroadcastPausedEvent)e;
            return new BroadcastPausedEventSchema(evt.BroadcastId.Value, evt.PausedAt.Value);
        });
        sink.RegisterPayloadMapper("BroadcastResumedEvent", e =>
        {
            var evt = (DomainEvents.BroadcastResumedEvent)e;
            return new BroadcastResumedEventSchema(evt.BroadcastId.Value, evt.ResumedAt.Value);
        });
        sink.RegisterPayloadMapper("BroadcastEndedEvent", e =>
        {
            var evt = (DomainEvents.BroadcastEndedEvent)e;
            return new BroadcastEndedEventSchema(evt.BroadcastId.Value, evt.EndedAt.Value);
        });
        sink.RegisterPayloadMapper("BroadcastCancelledEvent", e =>
        {
            var evt = (DomainEvents.BroadcastCancelledEvent)e;
            return new BroadcastCancelledEventSchema(evt.BroadcastId.Value, evt.Reason, evt.CancelledAt.Value);
        });
    }
}
