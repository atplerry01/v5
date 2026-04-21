using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Limit;
using DomainEvents = Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/entitlement/usage-control/limit domain.
/// </summary>
public sealed class BusinessEntitlementUsageControlLimitSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "LimitCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LimitCreatedEvent),
            typeof(LimitCreatedEventSchema));

        sink.RegisterSchema(
            "LimitEnforcedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LimitEnforcedEvent),
            typeof(LimitEnforcedEventSchema));

        sink.RegisterSchema(
            "LimitBreachedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LimitBreachedEvent),
            typeof(LimitBreachedEventSchema));

        sink.RegisterPayloadMapper("LimitCreatedEvent", e =>
        {
            var evt = (DomainEvents.LimitCreatedEvent)e;
            return new LimitCreatedEventSchema(evt.LimitId.Value, evt.SubjectId.Value, evt.ThresholdValue);
        });
        sink.RegisterPayloadMapper("LimitEnforcedEvent", e =>
        {
            var evt = (DomainEvents.LimitEnforcedEvent)e;
            return new LimitEnforcedEventSchema(evt.LimitId.Value);
        });
        sink.RegisterPayloadMapper("LimitBreachedEvent", e =>
        {
            var evt = (DomainEvents.LimitBreachedEvent)e;
            return new LimitBreachedEventSchema(evt.LimitId.Value, evt.ObservedValue);
        });
    }
}
