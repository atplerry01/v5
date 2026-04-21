using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Bundle;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/core-object/bundle BC. Binds domain
/// event CLR types to the <see cref="EventSchemaRegistry"/> and registers
/// payload mappers for projection-layer consumption.
/// </summary>
public sealed class ContentDocumentCoreObjectBundleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentBundleCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentBundleCreatedEvent), typeof(DocumentBundleCreatedEventSchema));
        sink.RegisterSchema("DocumentBundleRenamedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentBundleRenamedEvent), typeof(DocumentBundleRenamedEventSchema));
        sink.RegisterSchema("DocumentBundleMemberAddedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentBundleMemberAddedEvent), typeof(DocumentBundleMemberAddedEventSchema));
        sink.RegisterSchema("DocumentBundleMemberRemovedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentBundleMemberRemovedEvent), typeof(DocumentBundleMemberRemovedEventSchema));
        sink.RegisterSchema("DocumentBundleFinalizedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentBundleFinalizedEvent), typeof(DocumentBundleFinalizedEventSchema));
        sink.RegisterSchema("DocumentBundleArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentBundleArchivedEvent), typeof(DocumentBundleArchivedEventSchema));

        sink.RegisterPayloadMapper("DocumentBundleCreatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentBundleCreatedEvent)e;
            return new DocumentBundleCreatedEventSchema(
                evt.BundleId.Value,
                evt.Name.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentBundleRenamedEvent", e =>
        {
            var evt = (DomainEvents.DocumentBundleRenamedEvent)e;
            return new DocumentBundleRenamedEventSchema(
                evt.BundleId.Value,
                evt.PreviousName.Value,
                evt.NewName.Value,
                evt.RenamedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentBundleMemberAddedEvent", e =>
        {
            var evt = (DomainEvents.DocumentBundleMemberAddedEvent)e;
            return new DocumentBundleMemberAddedEventSchema(
                evt.BundleId.Value,
                evt.Member.Value,
                evt.AddedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentBundleMemberRemovedEvent", e =>
        {
            var evt = (DomainEvents.DocumentBundleMemberRemovedEvent)e;
            return new DocumentBundleMemberRemovedEventSchema(
                evt.BundleId.Value,
                evt.Member.Value,
                evt.RemovedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentBundleFinalizedEvent", e =>
        {
            var evt = (DomainEvents.DocumentBundleFinalizedEvent)e;
            return new DocumentBundleFinalizedEventSchema(
                evt.BundleId.Value,
                evt.FinalizedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentBundleArchivedEvent", e =>
        {
            var evt = (DomainEvents.DocumentBundleArchivedEvent)e;
            return new DocumentBundleArchivedEventSchema(
                evt.BundleId.Value,
                evt.ArchivedAt.Value);
        });
    }
}
