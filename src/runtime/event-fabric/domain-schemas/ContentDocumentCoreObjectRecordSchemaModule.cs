using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Record;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/core-object/record BC. Binds domain
/// event CLR types to the <see cref="EventSchemaRegistry"/> and registers
/// payload mappers for projection-layer consumption.
/// </summary>
public sealed class ContentDocumentCoreObjectRecordSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentRecordCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentRecordCreatedEvent), typeof(DocumentRecordCreatedEventSchema));
        sink.RegisterSchema("DocumentRecordLockedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentRecordLockedEvent), typeof(DocumentRecordLockedEventSchema));
        sink.RegisterSchema("DocumentRecordUnlockedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentRecordUnlockedEvent), typeof(DocumentRecordUnlockedEventSchema));
        sink.RegisterSchema("DocumentRecordClosedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentRecordClosedEvent), typeof(DocumentRecordClosedEventSchema));
        sink.RegisterSchema("DocumentRecordArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentRecordArchivedEvent), typeof(DocumentRecordArchivedEventSchema));

        sink.RegisterPayloadMapper("DocumentRecordCreatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentRecordCreatedEvent)e;
            return new DocumentRecordCreatedEventSchema(
                evt.RecordId.Value,
                evt.DocumentRef.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentRecordLockedEvent", e =>
        {
            var evt = (DomainEvents.DocumentRecordLockedEvent)e;
            return new DocumentRecordLockedEventSchema(
                evt.RecordId.Value,
                evt.Reason,
                evt.LockedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentRecordUnlockedEvent", e =>
        {
            var evt = (DomainEvents.DocumentRecordUnlockedEvent)e;
            return new DocumentRecordUnlockedEventSchema(
                evt.RecordId.Value,
                evt.UnlockedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentRecordClosedEvent", e =>
        {
            var evt = (DomainEvents.DocumentRecordClosedEvent)e;
            return new DocumentRecordClosedEventSchema(
                evt.RecordId.Value,
                evt.Reason.Value,
                evt.ClosedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentRecordArchivedEvent", e =>
        {
            var evt = (DomainEvents.DocumentRecordArchivedEvent)e;
            return new DocumentRecordArchivedEventSchema(
                evt.RecordId.Value,
                evt.ArchivedAt.Value);
        });
    }
}
