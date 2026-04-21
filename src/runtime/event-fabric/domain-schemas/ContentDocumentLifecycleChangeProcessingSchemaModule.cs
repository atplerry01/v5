using Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Processing;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/lifecycle-change/processing BC. Owns
/// binding from domain event CLR types to the <see cref="EventSchemaRegistry"/>
/// plus outbound payload mappers that project domain events into shared schema
/// records for the projection layer.
/// </summary>
public sealed class ContentDocumentLifecycleChangeProcessingSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentProcessingRequestedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentProcessingRequestedEvent), typeof(DocumentProcessingRequestedEventSchema));
        sink.RegisterSchema("DocumentProcessingStartedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentProcessingStartedEvent), typeof(DocumentProcessingStartedEventSchema));
        sink.RegisterSchema("DocumentProcessingCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentProcessingCompletedEvent), typeof(DocumentProcessingCompletedEventSchema));
        sink.RegisterSchema("DocumentProcessingFailedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentProcessingFailedEvent), typeof(DocumentProcessingFailedEventSchema));
        sink.RegisterSchema("DocumentProcessingCancelledEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentProcessingCancelledEvent), typeof(DocumentProcessingCancelledEventSchema));

        sink.RegisterPayloadMapper("DocumentProcessingRequestedEvent", e =>
        {
            var evt = (DomainEvents.DocumentProcessingRequestedEvent)e;
            return new DocumentProcessingRequestedEventSchema(
                evt.JobId.Value,
                evt.Kind.ToString(),
                evt.InputRef.Value,
                evt.RequestedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentProcessingStartedEvent", e =>
        {
            var evt = (DomainEvents.DocumentProcessingStartedEvent)e;
            return new DocumentProcessingStartedEventSchema(evt.JobId.Value, evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentProcessingCompletedEvent", e =>
        {
            var evt = (DomainEvents.DocumentProcessingCompletedEvent)e;
            return new DocumentProcessingCompletedEventSchema(
                evt.JobId.Value,
                evt.OutputRef.Value,
                evt.CompletedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentProcessingFailedEvent", e =>
        {
            var evt = (DomainEvents.DocumentProcessingFailedEvent)e;
            return new DocumentProcessingFailedEventSchema(
                evt.JobId.Value,
                evt.Reason.Value,
                evt.FailedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentProcessingCancelledEvent", e =>
        {
            var evt = (DomainEvents.DocumentProcessingCancelledEvent)e;
            return new DocumentProcessingCancelledEventSchema(evt.JobId.Value, evt.CancelledAt.Value);
        });
    }
}
