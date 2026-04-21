using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.UsageRight;
using DomainEvents = Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/entitlement/usage-control/usage-right domain.
///
/// Flattens the UsageRecord carrier on UsageRightUsedEvent into primitives on
/// the schema: RecordId and UnitsUsed ride the schema alongside the usual
/// AggregateId, which lets the projection track a running total without the
/// domain entity crossing the contracts boundary.
/// </summary>
public sealed class BusinessEntitlementUsageControlUsageRightSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "UsageRightCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.UsageRightCreatedEvent),
            typeof(UsageRightCreatedEventSchema));

        sink.RegisterSchema(
            "UsageRightUsedEvent",
            EventVersion.Default,
            typeof(DomainEvents.UsageRightUsedEvent),
            typeof(UsageRightUsedEventSchema));

        sink.RegisterSchema(
            "UsageRightConsumedEvent",
            EventVersion.Default,
            typeof(DomainEvents.UsageRightConsumedEvent),
            typeof(UsageRightConsumedEventSchema));

        sink.RegisterPayloadMapper("UsageRightCreatedEvent", e =>
        {
            var evt = (DomainEvents.UsageRightCreatedEvent)e;
            return new UsageRightCreatedEventSchema(
                evt.UsageRightId.Value,
                evt.SubjectId.Value,
                evt.ReferenceId.Value,
                evt.TotalUnits);
        });
        sink.RegisterPayloadMapper("UsageRightUsedEvent", e =>
        {
            var evt = (DomainEvents.UsageRightUsedEvent)e;
            return new UsageRightUsedEventSchema(
                evt.UsageRightId.Value,
                evt.RecordId.Value,
                evt.UnitsUsed);
        });
        sink.RegisterPayloadMapper("UsageRightConsumedEvent", e =>
        {
            var evt = (DomainEvents.UsageRightConsumedEvent)e;
            return new UsageRightConsumedEventSchema(evt.UsageRightId.Value);
        });
    }
}
