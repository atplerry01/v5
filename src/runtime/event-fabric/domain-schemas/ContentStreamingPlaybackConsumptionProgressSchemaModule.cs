using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Progress;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingPlaybackConsumptionProgressSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ProgressTrackedEvent", EventVersion.Default,
            typeof(DomainEvents.ProgressTrackedEvent), typeof(ProgressTrackedEventSchema));
        sink.RegisterSchema("PlaybackPositionUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackPositionUpdatedEvent), typeof(PlaybackPositionUpdatedEventSchema));
        sink.RegisterSchema("PlaybackPausedEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackPausedEvent), typeof(PlaybackPausedEventSchema));
        sink.RegisterSchema("PlaybackResumedEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackResumedEvent), typeof(PlaybackResumedEventSchema));
        sink.RegisterSchema("ProgressTerminatedEvent", EventVersion.Default,
            typeof(DomainEvents.ProgressTerminatedEvent), typeof(ProgressTerminatedEventSchema));

        sink.RegisterPayloadMapper("ProgressTrackedEvent", e =>
        {
            var evt = (DomainEvents.ProgressTrackedEvent)e;
            return new ProgressTrackedEventSchema(evt.ProgressId.Value, evt.SessionRef.Value, evt.Position.Milliseconds, evt.TrackedAt.Value);
        });
        sink.RegisterPayloadMapper("PlaybackPositionUpdatedEvent", e =>
        {
            var evt = (DomainEvents.PlaybackPositionUpdatedEvent)e;
            return new PlaybackPositionUpdatedEventSchema(evt.ProgressId.Value, evt.Position.Milliseconds, evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("PlaybackPausedEvent", e =>
        {
            var evt = (DomainEvents.PlaybackPausedEvent)e;
            return new PlaybackPausedEventSchema(evt.ProgressId.Value, evt.Position.Milliseconds, evt.PausedAt.Value);
        });
        sink.RegisterPayloadMapper("PlaybackResumedEvent", e =>
        {
            var evt = (DomainEvents.PlaybackResumedEvent)e;
            return new PlaybackResumedEventSchema(evt.ProgressId.Value, evt.ResumedAt.Value);
        });
        sink.RegisterPayloadMapper("ProgressTerminatedEvent", e =>
        {
            var evt = (DomainEvents.ProgressTerminatedEvent)e;
            return new ProgressTerminatedEventSchema(evt.ProgressId.Value, evt.TerminatedAt.Value);
        });
    }
}
