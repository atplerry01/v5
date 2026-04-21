using Whycespace.Shared.Contracts.Events.Content.Document.Intake.Upload;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.Intake.Upload;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/intake/upload BC. Owns binding from
/// domain event CLR types to the <see cref="EventSchemaRegistry"/> plus
/// outbound payload mappers that project domain events into shared schema
/// records for the projection layer.
/// </summary>
public sealed class ContentDocumentIntakeUploadSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentUploadRequestedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentUploadRequestedEvent), typeof(DocumentUploadRequestedEventSchema));
        sink.RegisterSchema("DocumentUploadAcceptedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentUploadAcceptedEvent), typeof(DocumentUploadAcceptedEventSchema));
        sink.RegisterSchema("DocumentUploadProcessingStartedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentUploadProcessingStartedEvent), typeof(DocumentUploadProcessingStartedEventSchema));
        sink.RegisterSchema("DocumentUploadCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentUploadCompletedEvent), typeof(DocumentUploadCompletedEventSchema));
        sink.RegisterSchema("DocumentUploadFailedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentUploadFailedEvent), typeof(DocumentUploadFailedEventSchema));
        sink.RegisterSchema("DocumentUploadCancelledEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentUploadCancelledEvent), typeof(DocumentUploadCancelledEventSchema));

        sink.RegisterPayloadMapper("DocumentUploadRequestedEvent", e =>
        {
            var evt = (DomainEvents.DocumentUploadRequestedEvent)e;
            return new DocumentUploadRequestedEventSchema(
                evt.UploadId.Value,
                evt.SourceRef.Value,
                evt.InputRef.Value,
                evt.RequestedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentUploadAcceptedEvent", e =>
        {
            var evt = (DomainEvents.DocumentUploadAcceptedEvent)e;
            return new DocumentUploadAcceptedEventSchema(evt.UploadId.Value, evt.AcceptedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentUploadProcessingStartedEvent", e =>
        {
            var evt = (DomainEvents.DocumentUploadProcessingStartedEvent)e;
            return new DocumentUploadProcessingStartedEventSchema(evt.UploadId.Value, evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentUploadCompletedEvent", e =>
        {
            var evt = (DomainEvents.DocumentUploadCompletedEvent)e;
            return new DocumentUploadCompletedEventSchema(
                evt.UploadId.Value,
                evt.OutputRef.Value,
                evt.CompletedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentUploadFailedEvent", e =>
        {
            var evt = (DomainEvents.DocumentUploadFailedEvent)e;
            return new DocumentUploadFailedEventSchema(
                evt.UploadId.Value,
                evt.Reason.Value,
                evt.FailedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentUploadCancelledEvent", e =>
        {
            var evt = (DomainEvents.DocumentUploadCancelledEvent)e;
            return new DocumentUploadCancelledEventSchema(evt.UploadId.Value, evt.CancelledAt.Value);
        });
    }
}
