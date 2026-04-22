using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Replay;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingPlaybackConsumptionReplaySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ReplayRequestedEvent", EventVersion.Default,
            typeof(DomainEvents.ReplayRequestedEvent), typeof(ReplayRequestedEventSchema));
        sink.RegisterSchema("ReplayStartedEvent", EventVersion.Default,
            typeof(DomainEvents.ReplayStartedEvent), typeof(ReplayStartedEventSchema));
        sink.RegisterSchema("ReplayPausedEvent", EventVersion.Default,
            typeof(DomainEvents.ReplayPausedEvent), typeof(ReplayPausedEventSchema));
        sink.RegisterSchema("ReplayResumedEvent", EventVersion.Default,
            typeof(DomainEvents.ReplayResumedEvent), typeof(ReplayResumedEventSchema));
        sink.RegisterSchema("ReplayCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.ReplayCompletedEvent), typeof(ReplayCompletedEventSchema));
        sink.RegisterSchema("ReplayAbandonedEvent", EventVersion.Default,
            typeof(DomainEvents.ReplayAbandonedEvent), typeof(ReplayAbandonedEventSchema));

        sink.RegisterPayloadMapper("ReplayRequestedEvent", e =>
        {
            var evt = (DomainEvents.ReplayRequestedEvent)e;
            return new ReplayRequestedEventSchema(evt.ReplayId.Value, evt.ArchiveRef.Value, evt.ViewerRef.Value, evt.RequestedAt.Value);
        });
        sink.RegisterPayloadMapper("ReplayStartedEvent", e =>
        {
            var evt = (DomainEvents.ReplayStartedEvent)e;
            return new ReplayStartedEventSchema(evt.ReplayId.Value, evt.Position.Milliseconds, evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("ReplayPausedEvent", e =>
        {
            var evt = (DomainEvents.ReplayPausedEvent)e;
            return new ReplayPausedEventSchema(evt.ReplayId.Value, evt.Position.Milliseconds, evt.PausedAt.Value);
        });
        sink.RegisterPayloadMapper("ReplayResumedEvent", e =>
        {
            var evt = (DomainEvents.ReplayResumedEvent)e;
            return new ReplayResumedEventSchema(evt.ReplayId.Value, evt.ResumedAt.Value);
        });
        sink.RegisterPayloadMapper("ReplayCompletedEvent", e =>
        {
            var evt = (DomainEvents.ReplayCompletedEvent)e;
            return new ReplayCompletedEventSchema(evt.ReplayId.Value, evt.Position.Milliseconds, evt.CompletedAt.Value);
        });
        sink.RegisterPayloadMapper("ReplayAbandonedEvent", e =>
        {
            var evt = (DomainEvents.ReplayAbandonedEvent)e;
            return new ReplayAbandonedEventSchema(evt.ReplayId.Value, evt.AbandonedAt.Value);
        });
    }
}
