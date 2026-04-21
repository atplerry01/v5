using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Assignment;
using DomainEvents = Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/entitlement/eligibility-and-grant/assignment domain.
///
/// Owns the binding from Assignment domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed AssignmentId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessEntitlementEligibilityAndGrantAssignmentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AssignmentCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AssignmentCreatedEvent),
            typeof(AssignmentCreatedEventSchema));

        sink.RegisterSchema(
            "AssignmentActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AssignmentActivatedEvent),
            typeof(AssignmentActivatedEventSchema));

        sink.RegisterSchema(
            "AssignmentRevokedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AssignmentRevokedEvent),
            typeof(AssignmentRevokedEventSchema));

        sink.RegisterPayloadMapper("AssignmentCreatedEvent", e =>
        {
            var evt = (DomainEvents.AssignmentCreatedEvent)e;
            return new AssignmentCreatedEventSchema(
                evt.AssignmentId.Value,
                evt.Grant.Value,
                evt.Subject.Value,
                evt.Scope.Value);
        });
        sink.RegisterPayloadMapper("AssignmentActivatedEvent", e =>
        {
            var evt = (DomainEvents.AssignmentActivatedEvent)e;
            return new AssignmentActivatedEventSchema(evt.AssignmentId.Value, evt.ActivatedAt);
        });
        sink.RegisterPayloadMapper("AssignmentRevokedEvent", e =>
        {
            var evt = (DomainEvents.AssignmentRevokedEvent)e;
            return new AssignmentRevokedEventSchema(evt.AssignmentId.Value, evt.RevokedAt);
        });
    }
}
