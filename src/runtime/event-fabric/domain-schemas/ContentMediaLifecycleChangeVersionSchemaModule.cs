using Whycespace.Shared.Contracts.Events.Content.Media.LifecycleChange.Version;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentMediaLifecycleChangeVersionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MediaVersionCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaVersionCreatedEvent), typeof(MediaVersionCreatedEventSchema));
        sink.RegisterSchema("MediaVersionActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaVersionActivatedEvent), typeof(MediaVersionActivatedEventSchema));
        sink.RegisterSchema("MediaVersionSupersededEvent", EventVersion.Default,
            typeof(DomainEvents.MediaVersionSupersededEvent), typeof(MediaVersionSupersededEventSchema));
        sink.RegisterSchema("MediaVersionWithdrawnEvent", EventVersion.Default,
            typeof(DomainEvents.MediaVersionWithdrawnEvent), typeof(MediaVersionWithdrawnEventSchema));

        sink.RegisterPayloadMapper("MediaVersionCreatedEvent", e =>
        {
            var evt = (DomainEvents.MediaVersionCreatedEvent)e;
            return new MediaVersionCreatedEventSchema(
                evt.VersionId.Value,
                evt.AssetRef.Value,
                evt.VersionNumber.Major,
                evt.VersionNumber.Minor,
                evt.FileRef.Value,
                evt.PreviousVersionId?.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaVersionActivatedEvent", e =>
        {
            var evt = (DomainEvents.MediaVersionActivatedEvent)e;
            return new MediaVersionActivatedEventSchema(evt.VersionId.Value, evt.ActivatedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaVersionSupersededEvent", e =>
        {
            var evt = (DomainEvents.MediaVersionSupersededEvent)e;
            return new MediaVersionSupersededEventSchema(evt.VersionId.Value, evt.SuccessorVersionId.Value, evt.SupersededAt.Value);
        });
        sink.RegisterPayloadMapper("MediaVersionWithdrawnEvent", e =>
        {
            var evt = (DomainEvents.MediaVersionWithdrawnEvent)e;
            return new MediaVersionWithdrawnEventSchema(evt.VersionId.Value, evt.Reason, evt.WithdrawnAt.Value);
        });
    }
}
