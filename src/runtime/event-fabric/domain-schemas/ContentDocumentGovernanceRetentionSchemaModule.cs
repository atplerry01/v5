using Whycespace.Shared.Contracts.Events.Content.Document.Governance.Retention;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.Governance.Retention;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/governance/retention BC. Owns
/// binding from domain event CLR types to the <see cref="EventSchemaRegistry"/>
/// plus outbound payload mappers that project domain events into shared
/// schema records for the projection layer.
/// </summary>
public sealed class ContentDocumentGovernanceRetentionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RetentionAppliedEvent", EventVersion.Default,
            typeof(DomainEvents.RetentionAppliedEvent), typeof(RetentionAppliedEventSchema));
        sink.RegisterSchema("RetentionHoldPlacedEvent", EventVersion.Default,
            typeof(DomainEvents.RetentionHoldPlacedEvent), typeof(RetentionHoldPlacedEventSchema));
        sink.RegisterSchema("RetentionReleasedEvent", EventVersion.Default,
            typeof(DomainEvents.RetentionReleasedEvent), typeof(RetentionReleasedEventSchema));
        sink.RegisterSchema("RetentionExpiredEvent", EventVersion.Default,
            typeof(DomainEvents.RetentionExpiredEvent), typeof(RetentionExpiredEventSchema));
        sink.RegisterSchema("RetentionMarkedEligibleForDestructionEvent", EventVersion.Default,
            typeof(DomainEvents.RetentionMarkedEligibleForDestructionEvent), typeof(RetentionMarkedEligibleForDestructionEventSchema));
        sink.RegisterSchema("RetentionArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.RetentionArchivedEvent), typeof(RetentionArchivedEventSchema));

        sink.RegisterPayloadMapper("RetentionAppliedEvent", e =>
        {
            var evt = (DomainEvents.RetentionAppliedEvent)e;
            return new RetentionAppliedEventSchema(
                evt.RetentionId.Value,
                evt.TargetRef.Value,
                evt.TargetRef.Kind.ToString(),
                evt.Window.AppliedAt.Value,
                evt.Window.ExpiresAt.Value,
                evt.Reason.Value,
                evt.AppliedAt.Value);
        });
        sink.RegisterPayloadMapper("RetentionHoldPlacedEvent", e =>
        {
            var evt = (DomainEvents.RetentionHoldPlacedEvent)e;
            return new RetentionHoldPlacedEventSchema(
                evt.RetentionId.Value,
                evt.Reason.Value,
                evt.PlacedAt.Value);
        });
        sink.RegisterPayloadMapper("RetentionReleasedEvent", e =>
        {
            var evt = (DomainEvents.RetentionReleasedEvent)e;
            return new RetentionReleasedEventSchema(evt.RetentionId.Value, evt.ReleasedAt.Value);
        });
        sink.RegisterPayloadMapper("RetentionExpiredEvent", e =>
        {
            var evt = (DomainEvents.RetentionExpiredEvent)e;
            return new RetentionExpiredEventSchema(evt.RetentionId.Value, evt.ExpiredAt.Value);
        });
        sink.RegisterPayloadMapper("RetentionMarkedEligibleForDestructionEvent", e =>
        {
            var evt = (DomainEvents.RetentionMarkedEligibleForDestructionEvent)e;
            return new RetentionMarkedEligibleForDestructionEventSchema(evt.RetentionId.Value, evt.MarkedAt.Value);
        });
        sink.RegisterPayloadMapper("RetentionArchivedEvent", e =>
        {
            var evt = (DomainEvents.RetentionArchivedEvent)e;
            return new RetentionArchivedEventSchema(evt.RetentionId.Value, evt.ArchivedAt.Value);
        });
    }
}
