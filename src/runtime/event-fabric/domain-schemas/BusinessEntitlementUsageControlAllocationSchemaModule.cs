using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Allocation;
using DomainEvents = Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/entitlement/usage-control/allocation domain.
/// </summary>
public sealed class BusinessEntitlementUsageControlAllocationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AllocationCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AllocationCreatedEvent),
            typeof(AllocationCreatedEventSchema));

        sink.RegisterSchema(
            "AllocationAllocatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AllocationAllocatedEvent),
            typeof(AllocationAllocatedEventSchema));

        sink.RegisterSchema(
            "AllocationReleasedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AllocationReleasedEvent),
            typeof(AllocationReleasedEventSchema));

        sink.RegisterPayloadMapper("AllocationCreatedEvent", e =>
        {
            var evt = (DomainEvents.AllocationCreatedEvent)e;
            return new AllocationCreatedEventSchema(evt.AllocationId.Value, evt.ResourceId.Value, evt.RequestedCapacity);
        });
        sink.RegisterPayloadMapper("AllocationAllocatedEvent", e =>
        {
            var evt = (DomainEvents.AllocationAllocatedEvent)e;
            return new AllocationAllocatedEventSchema(evt.AllocationId.Value);
        });
        sink.RegisterPayloadMapper("AllocationReleasedEvent", e =>
        {
            var evt = (DomainEvents.AllocationReleasedEvent)e;
            return new AllocationReleasedEventSchema(evt.AllocationId.Value);
        });
    }
}
