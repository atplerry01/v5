using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using DomainEvents = Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/customer/segmentation-and-lifecycle/lifecycle domain.
///
/// Owns the binding from Lifecycle domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed LifecycleId) into the shared
/// schema records (Guid AggregateId) consumed by the projection layer.
/// </summary>
public sealed class BusinessCustomerSegmentationAndLifecycleLifecycleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "LifecycleStartedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LifecycleStartedEvent),
            typeof(LifecycleStartedEventSchema));

        sink.RegisterSchema(
            "LifecycleStageChangedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LifecycleStageChangedEvent),
            typeof(LifecycleStageChangedEventSchema));

        sink.RegisterSchema(
            "LifecycleClosedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LifecycleClosedEvent),
            typeof(LifecycleClosedEventSchema));

        sink.RegisterPayloadMapper("LifecycleStartedEvent", e =>
        {
            var evt = (DomainEvents.LifecycleStartedEvent)e;
            return new LifecycleStartedEventSchema(
                evt.LifecycleId.Value,
                evt.Customer.Value,
                evt.InitialStage.ToString(),
                evt.StartedAt);
        });
        sink.RegisterPayloadMapper("LifecycleStageChangedEvent", e =>
        {
            var evt = (DomainEvents.LifecycleStageChangedEvent)e;
            return new LifecycleStageChangedEventSchema(
                evt.LifecycleId.Value,
                evt.From.ToString(),
                evt.To.ToString(),
                evt.ChangedAt);
        });
        sink.RegisterPayloadMapper("LifecycleClosedEvent", e =>
        {
            var evt = (DomainEvents.LifecycleClosedEvent)e;
            return new LifecycleClosedEventSchema(evt.LifecycleId.Value, evt.ClosedAt);
        });
    }
}
