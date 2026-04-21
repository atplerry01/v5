using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Eligibility;
using DomainEvents = Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/entitlement/eligibility-and-grant/eligibility domain.
///
/// Owns the binding from Eligibility domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed EligibilityId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessEntitlementEligibilityAndGrantEligibilitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "EligibilityCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.EligibilityCreatedEvent),
            typeof(EligibilityCreatedEventSchema));

        sink.RegisterSchema(
            "EligibilityEvaluatedEligibleEvent",
            EventVersion.Default,
            typeof(DomainEvents.EligibilityEvaluatedEligibleEvent),
            typeof(EligibilityEvaluatedEligibleEventSchema));

        sink.RegisterSchema(
            "EligibilityEvaluatedIneligibleEvent",
            EventVersion.Default,
            typeof(DomainEvents.EligibilityEvaluatedIneligibleEvent),
            typeof(EligibilityEvaluatedIneligibleEventSchema));

        sink.RegisterPayloadMapper("EligibilityCreatedEvent", e =>
        {
            var evt = (DomainEvents.EligibilityCreatedEvent)e;
            return new EligibilityCreatedEventSchema(
                evt.EligibilityId.Value,
                evt.Subject.Value,
                evt.Target.Value,
                evt.Scope.Value);
        });
        sink.RegisterPayloadMapper("EligibilityEvaluatedEligibleEvent", e =>
        {
            var evt = (DomainEvents.EligibilityEvaluatedEligibleEvent)e;
            return new EligibilityEvaluatedEligibleEventSchema(evt.EligibilityId.Value, evt.EvaluatedAt);
        });
        sink.RegisterPayloadMapper("EligibilityEvaluatedIneligibleEvent", e =>
        {
            var evt = (DomainEvents.EligibilityEvaluatedIneligibleEvent)e;
            return new EligibilityEvaluatedIneligibleEventSchema(
                evt.EligibilityId.Value,
                evt.Reason.Value,
                evt.EvaluatedAt);
        });
    }
}
