using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Availability;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingStreamCoreAvailabilitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PlaybackCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackCreatedEvent), typeof(PlaybackCreatedEventSchema));
        sink.RegisterSchema("PlaybackEnabledEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackEnabledEvent), typeof(PlaybackEnabledEventSchema));
        sink.RegisterSchema("PlaybackDisabledEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackDisabledEvent), typeof(PlaybackDisabledEventSchema));
        sink.RegisterSchema("PlaybackWindowUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackWindowUpdatedEvent), typeof(PlaybackWindowUpdatedEventSchema));
        sink.RegisterSchema("PlaybackArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.PlaybackArchivedEvent), typeof(PlaybackArchivedEventSchema));

        sink.RegisterPayloadMapper("PlaybackCreatedEvent", e =>
        {
            var evt = (DomainEvents.PlaybackCreatedEvent)e;
            return new PlaybackCreatedEventSchema(
                evt.PlaybackId.Value,
                evt.SourceRef.Value,
                evt.SourceRef.Kind.ToString(),
                evt.Mode.ToString(),
                evt.Window.AvailableFrom.Value,
                evt.Window.AvailableUntil.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("PlaybackEnabledEvent", e =>
        {
            var evt = (DomainEvents.PlaybackEnabledEvent)e;
            return new PlaybackEnabledEventSchema(evt.PlaybackId.Value, evt.EnabledAt.Value);
        });
        sink.RegisterPayloadMapper("PlaybackDisabledEvent", e =>
        {
            var evt = (DomainEvents.PlaybackDisabledEvent)e;
            return new PlaybackDisabledEventSchema(evt.PlaybackId.Value, evt.Reason, evt.DisabledAt.Value);
        });
        sink.RegisterPayloadMapper("PlaybackWindowUpdatedEvent", e =>
        {
            var evt = (DomainEvents.PlaybackWindowUpdatedEvent)e;
            return new PlaybackWindowUpdatedEventSchema(
                evt.PlaybackId.Value,
                evt.PreviousWindow.AvailableFrom.Value,
                evt.PreviousWindow.AvailableUntil.Value,
                evt.NewWindow.AvailableFrom.Value,
                evt.NewWindow.AvailableUntil.Value,
                evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("PlaybackArchivedEvent", e =>
        {
            var evt = (DomainEvents.PlaybackArchivedEvent)e;
            return new PlaybackArchivedEventSchema(evt.PlaybackId.Value, evt.ArchivedAt.Value);
        });
    }
}
