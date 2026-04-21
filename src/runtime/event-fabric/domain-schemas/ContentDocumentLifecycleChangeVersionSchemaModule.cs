using Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Version;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/lifecycle-change/version BC. Owns
/// binding from domain event CLR types to the <see cref="EventSchemaRegistry"/>
/// plus outbound payload mappers that project domain events into shared schema
/// records for the projection layer.
/// </summary>
public sealed class ContentDocumentLifecycleChangeVersionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentVersionCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentVersionCreatedEvent), typeof(DocumentVersionCreatedEventSchema));
        sink.RegisterSchema("DocumentVersionActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentVersionActivatedEvent), typeof(DocumentVersionActivatedEventSchema));
        sink.RegisterSchema("DocumentVersionSupersededEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentVersionSupersededEvent), typeof(DocumentVersionSupersededEventSchema));
        sink.RegisterSchema("DocumentVersionWithdrawnEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentVersionWithdrawnEvent), typeof(DocumentVersionWithdrawnEventSchema));

        sink.RegisterPayloadMapper("DocumentVersionCreatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentVersionCreatedEvent)e;
            return new DocumentVersionCreatedEventSchema(
                evt.VersionId.Value,
                evt.DocumentRef.Value,
                evt.VersionNumber.Major,
                evt.VersionNumber.Minor,
                evt.ArtifactRef.Value,
                evt.PreviousVersionId?.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentVersionActivatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentVersionActivatedEvent)e;
            return new DocumentVersionActivatedEventSchema(evt.VersionId.Value, evt.ActivatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentVersionSupersededEvent", e =>
        {
            var evt = (DomainEvents.DocumentVersionSupersededEvent)e;
            return new DocumentVersionSupersededEventSchema(
                evt.VersionId.Value,
                evt.SuccessorVersionId.Value,
                evt.SupersededAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentVersionWithdrawnEvent", e =>
        {
            var evt = (DomainEvents.DocumentVersionWithdrawnEvent)e;
            return new DocumentVersionWithdrawnEventSchema(
                evt.VersionId.Value,
                evt.Reason,
                evt.WithdrawnAt.Value);
        });
    }
}
