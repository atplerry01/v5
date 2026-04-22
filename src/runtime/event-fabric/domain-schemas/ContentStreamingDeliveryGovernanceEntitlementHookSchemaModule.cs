using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.EntitlementHook;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingDeliveryGovernanceEntitlementHookSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("EntitlementHookRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.EntitlementHookRegisteredEvent), typeof(EntitlementHookRegisteredEventSchema));
        sink.RegisterSchema("EntitlementQueriedEvent", EventVersion.Default,
            typeof(DomainEvents.EntitlementQueriedEvent), typeof(EntitlementQueriedEventSchema));
        sink.RegisterSchema("EntitlementRefreshedEvent", EventVersion.Default,
            typeof(DomainEvents.EntitlementRefreshedEvent), typeof(EntitlementRefreshedEventSchema));
        sink.RegisterSchema("EntitlementInvalidatedEvent", EventVersion.Default,
            typeof(DomainEvents.EntitlementInvalidatedEvent), typeof(EntitlementInvalidatedEventSchema));
        sink.RegisterSchema("EntitlementFailureRecordedEvent", EventVersion.Default,
            typeof(DomainEvents.EntitlementFailureRecordedEvent), typeof(EntitlementFailureRecordedEventSchema));

        sink.RegisterPayloadMapper("EntitlementHookRegisteredEvent", e =>
        {
            var evt = (DomainEvents.EntitlementHookRegisteredEvent)e;
            return new EntitlementHookRegisteredEventSchema(evt.HookId.Value, evt.TargetRef.Value, evt.SourceSystem.Value, evt.RegisteredAt.Value);
        });
        sink.RegisterPayloadMapper("EntitlementQueriedEvent", e =>
        {
            var evt = (DomainEvents.EntitlementQueriedEvent)e;
            return new EntitlementQueriedEventSchema(evt.HookId.Value, evt.Result.ToString(), evt.QueriedAt.Value);
        });
        sink.RegisterPayloadMapper("EntitlementRefreshedEvent", e =>
        {
            var evt = (DomainEvents.EntitlementRefreshedEvent)e;
            return new EntitlementRefreshedEventSchema(evt.HookId.Value, evt.Result.ToString(), evt.RefreshedAt.Value);
        });
        sink.RegisterPayloadMapper("EntitlementInvalidatedEvent", e =>
        {
            var evt = (DomainEvents.EntitlementInvalidatedEvent)e;
            return new EntitlementInvalidatedEventSchema(evt.HookId.Value, evt.InvalidatedAt.Value);
        });
        sink.RegisterPayloadMapper("EntitlementFailureRecordedEvent", e =>
        {
            var evt = (DomainEvents.EntitlementFailureRecordedEvent)e;
            return new EntitlementFailureRecordedEventSchema(evt.HookId.Value, evt.Reason, evt.FailedAt.Value);
        });
    }
}
