using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Manifest;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingStreamCoreManifestSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ManifestCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.ManifestCreatedEvent), typeof(ManifestCreatedEventSchema));
        sink.RegisterSchema("ManifestUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.ManifestUpdatedEvent), typeof(ManifestUpdatedEventSchema));
        sink.RegisterSchema("ManifestPublishedEvent", EventVersion.Default,
            typeof(DomainEvents.ManifestPublishedEvent), typeof(ManifestPublishedEventSchema));
        sink.RegisterSchema("ManifestRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.ManifestRetiredEvent), typeof(ManifestRetiredEventSchema));
        sink.RegisterSchema("ManifestArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.ManifestArchivedEvent), typeof(ManifestArchivedEventSchema));

        sink.RegisterPayloadMapper("ManifestCreatedEvent", e =>
        {
            var evt = (DomainEvents.ManifestCreatedEvent)e;
            return new ManifestCreatedEventSchema(evt.ManifestId.Value, evt.SourceRef.Value, evt.SourceRef.Kind.ToString(), evt.InitialVersion.Value, evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("ManifestUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ManifestUpdatedEvent)e;
            return new ManifestUpdatedEventSchema(evt.ManifestId.Value, evt.PreviousVersion.Value, evt.NewVersion.Value, evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("ManifestPublishedEvent", e =>
        {
            var evt = (DomainEvents.ManifestPublishedEvent)e;
            return new ManifestPublishedEventSchema(evt.ManifestId.Value, evt.Version.Value, evt.PublishedAt.Value);
        });
        sink.RegisterPayloadMapper("ManifestRetiredEvent", e =>
        {
            var evt = (DomainEvents.ManifestRetiredEvent)e;
            return new ManifestRetiredEventSchema(evt.ManifestId.Value, evt.Reason, evt.RetiredAt.Value);
        });
        sink.RegisterPayloadMapper("ManifestArchivedEvent", e =>
        {
            var evt = (DomainEvents.ManifestArchivedEvent)e;
            return new ManifestArchivedEventSchema(evt.ManifestId.Value, evt.ArchivedAt.Value);
        });
    }
}
