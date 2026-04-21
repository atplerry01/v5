using Whycespace.Shared.Contracts.Events.Content.Media.Intake.Ingest;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentMediaIntakeIngestSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MediaIngestRequestedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaIngestRequestedEvent), typeof(MediaIngestRequestedEventSchema));
        sink.RegisterSchema("MediaIngestAcceptedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaIngestAcceptedEvent), typeof(MediaIngestAcceptedEventSchema));
        sink.RegisterSchema("MediaIngestProcessingStartedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaIngestProcessingStartedEvent), typeof(MediaIngestProcessingStartedEventSchema));
        sink.RegisterSchema("MediaIngestCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaIngestCompletedEvent), typeof(MediaIngestCompletedEventSchema));
        sink.RegisterSchema("MediaIngestFailedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaIngestFailedEvent), typeof(MediaIngestFailedEventSchema));
        sink.RegisterSchema("MediaIngestCancelledEvent", EventVersion.Default,
            typeof(DomainEvents.MediaIngestCancelledEvent), typeof(MediaIngestCancelledEventSchema));

        sink.RegisterPayloadMapper("MediaIngestRequestedEvent", e =>
        {
            var evt = (DomainEvents.MediaIngestRequestedEvent)e;
            return new MediaIngestRequestedEventSchema(evt.UploadId.Value, evt.SourceRef.Value, evt.InputRef.Value, evt.RequestedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaIngestAcceptedEvent", e =>
        {
            var evt = (DomainEvents.MediaIngestAcceptedEvent)e;
            return new MediaIngestAcceptedEventSchema(evt.UploadId.Value, evt.AcceptedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaIngestProcessingStartedEvent", e =>
        {
            var evt = (DomainEvents.MediaIngestProcessingStartedEvent)e;
            return new MediaIngestProcessingStartedEventSchema(evt.UploadId.Value, evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaIngestCompletedEvent", e =>
        {
            var evt = (DomainEvents.MediaIngestCompletedEvent)e;
            return new MediaIngestCompletedEventSchema(evt.UploadId.Value, evt.OutputRef.Value, evt.CompletedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaIngestFailedEvent", e =>
        {
            var evt = (DomainEvents.MediaIngestFailedEvent)e;
            return new MediaIngestFailedEventSchema(evt.UploadId.Value, evt.Reason.Value, evt.FailedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaIngestCancelledEvent", e =>
        {
            var evt = (DomainEvents.MediaIngestCancelledEvent)e;
            return new MediaIngestCancelledEventSchema(evt.UploadId.Value, evt.CancelledAt.Value);
        });
    }
}
