using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Plan;
using DomainEvents = Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/offering/commercial-shape/plan domain.
///
/// Owns the binding from Plan domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed PlanId + PlanDescriptor VO) into
/// the shared schema records (Guid AggregateId + flattened PlanName/PlanTier
/// pair) consumed by the projection layer.
/// </summary>
public sealed class BusinessOfferingCommercialShapePlanSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "PlanDraftedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PlanDraftedEvent),
            typeof(PlanDraftedEventSchema));

        sink.RegisterSchema(
            "PlanActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PlanActivatedEvent),
            typeof(PlanActivatedEventSchema));

        sink.RegisterSchema(
            "PlanDeprecatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PlanDeprecatedEvent),
            typeof(PlanDeprecatedEventSchema));

        sink.RegisterSchema(
            "PlanArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PlanArchivedEvent),
            typeof(PlanArchivedEventSchema));

        sink.RegisterPayloadMapper("PlanDraftedEvent", e =>
        {
            var evt = (DomainEvents.PlanDraftedEvent)e;
            return new PlanDraftedEventSchema(evt.PlanId.Value, evt.Descriptor.PlanName, evt.Descriptor.PlanTier);
        });
        sink.RegisterPayloadMapper("PlanActivatedEvent", e =>
        {
            var evt = (DomainEvents.PlanActivatedEvent)e;
            return new PlanActivatedEventSchema(evt.PlanId.Value);
        });
        sink.RegisterPayloadMapper("PlanDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.PlanDeprecatedEvent)e;
            return new PlanDeprecatedEventSchema(evt.PlanId.Value);
        });
        sink.RegisterPayloadMapper("PlanArchivedEvent", e =>
        {
            var evt = (DomainEvents.PlanArchivedEvent)e;
            return new PlanArchivedEventSchema(evt.PlanId.Value);
        });
    }
}
