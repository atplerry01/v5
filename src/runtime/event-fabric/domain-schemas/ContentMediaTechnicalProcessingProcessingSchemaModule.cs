using Whycespace.Shared.Contracts.Events.Content.Media.TechnicalProcessing.Processing;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentMediaTechnicalProcessingProcessingSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MediaProcessingRequestedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaProcessingRequestedEvent), typeof(MediaProcessingRequestedEventSchema));
        sink.RegisterSchema("MediaProcessingStartedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaProcessingStartedEvent), typeof(MediaProcessingStartedEventSchema));
        sink.RegisterSchema("MediaProcessingCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaProcessingCompletedEvent), typeof(MediaProcessingCompletedEventSchema));
        sink.RegisterSchema("MediaProcessingFailedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaProcessingFailedEvent), typeof(MediaProcessingFailedEventSchema));
        sink.RegisterSchema("MediaProcessingCancelledEvent", EventVersion.Default,
            typeof(DomainEvents.MediaProcessingCancelledEvent), typeof(MediaProcessingCancelledEventSchema));

        sink.RegisterPayloadMapper("MediaProcessingRequestedEvent", e =>
        {
            var evt = (DomainEvents.MediaProcessingRequestedEvent)e;
            return new MediaProcessingRequestedEventSchema(evt.JobId.Value, evt.Kind.ToString(), evt.InputRef.Value, evt.RequestedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaProcessingStartedEvent", e =>
        {
            var evt = (DomainEvents.MediaProcessingStartedEvent)e;
            return new MediaProcessingStartedEventSchema(evt.JobId.Value, evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaProcessingCompletedEvent", e =>
        {
            var evt = (DomainEvents.MediaProcessingCompletedEvent)e;
            return new MediaProcessingCompletedEventSchema(evt.JobId.Value, evt.OutputRef.Value, evt.CompletedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaProcessingFailedEvent", e =>
        {
            var evt = (DomainEvents.MediaProcessingFailedEvent)e;
            return new MediaProcessingFailedEventSchema(evt.JobId.Value, evt.Reason.Value, evt.FailedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaProcessingCancelledEvent", e =>
        {
            var evt = (DomainEvents.MediaProcessingCancelledEvent)e;
            return new MediaProcessingCancelledEventSchema(evt.JobId.Value, evt.CancelledAt.Value);
        });
    }
}
