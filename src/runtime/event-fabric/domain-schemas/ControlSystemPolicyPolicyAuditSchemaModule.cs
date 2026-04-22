using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyAudit;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemPolicyPolicyAuditSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PolicyAuditEntryRecordedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyAuditEntryRecordedEvent), typeof(PolicyAuditEntryRecordedEventSchema));
        sink.RegisterSchema("PolicyAuditEntryReviewedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyAuditEntryReviewedEvent), typeof(PolicyAuditEntryReviewedEventSchema));

        sink.RegisterPayloadMapper("PolicyAuditEntryRecordedEvent", e =>
        {
            var evt = (DomainEvents.PolicyAuditEntryRecordedEvent)e;
            return new PolicyAuditEntryRecordedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.PolicyId,
                evt.ActorId,
                evt.Action,
                evt.Category.ToString(),
                evt.DecisionHash,
                evt.CorrelationId,
                evt.OccurredAt);
        });
        sink.RegisterPayloadMapper("PolicyAuditEntryReviewedEvent", e =>
        {
            var evt = (DomainEvents.PolicyAuditEntryReviewedEvent)e;
            return new PolicyAuditEntryReviewedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ReviewerId,
                evt.Reason);
        });
    }
}
