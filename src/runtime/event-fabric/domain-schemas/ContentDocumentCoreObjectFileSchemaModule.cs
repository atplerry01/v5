using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.File;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.CoreObject.File;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/core-object/file BC. Binds domain
/// event CLR types to the <see cref="EventSchemaRegistry"/> and registers
/// payload mappers for projection-layer consumption.
/// </summary>
public sealed class ContentDocumentCoreObjectFileSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentFileRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentFileRegisteredEvent), typeof(DocumentFileRegisteredEventSchema));
        sink.RegisterSchema("DocumentFileIntegrityVerifiedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentFileIntegrityVerifiedEvent), typeof(DocumentFileIntegrityVerifiedEventSchema));
        sink.RegisterSchema("DocumentFileSupersededEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentFileSupersededEvent), typeof(DocumentFileSupersededEventSchema));
        sink.RegisterSchema("DocumentFileArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentFileArchivedEvent), typeof(DocumentFileArchivedEventSchema));

        sink.RegisterPayloadMapper("DocumentFileRegisteredEvent", e =>
        {
            var evt = (DomainEvents.DocumentFileRegisteredEvent)e;
            return new DocumentFileRegisteredEventSchema(
                evt.DocumentFileId.Value,
                evt.DocumentRef.Value,
                evt.StorageRef.Value,
                evt.DeclaredChecksum.Value,
                evt.MimeType.Value,
                evt.Size.Bytes,
                evt.RegisteredAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentFileIntegrityVerifiedEvent", e =>
        {
            var evt = (DomainEvents.DocumentFileIntegrityVerifiedEvent)e;
            return new DocumentFileIntegrityVerifiedEventSchema(
                evt.DocumentFileId.Value,
                evt.VerifiedChecksum.Value,
                evt.VerifiedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentFileSupersededEvent", e =>
        {
            var evt = (DomainEvents.DocumentFileSupersededEvent)e;
            return new DocumentFileSupersededEventSchema(
                evt.DocumentFileId.Value,
                evt.SuccessorFileId.Value,
                evt.SupersededAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentFileArchivedEvent", e =>
        {
            var evt = (DomainEvents.DocumentFileArchivedEvent)e;
            return new DocumentFileArchivedEventSchema(
                evt.DocumentFileId.Value,
                evt.ArchivedAt.Value);
        });
    }
}
