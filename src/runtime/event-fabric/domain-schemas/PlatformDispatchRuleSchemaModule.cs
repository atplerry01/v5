using Whycespace.Shared.Contracts.Events.Platform.Routing.DispatchRule;
using DomainEvents = Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformDispatchRuleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DispatchRuleRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.DispatchRuleRegisteredEvent), typeof(DispatchRuleRegisteredEventSchema));
        sink.RegisterSchema("DispatchRuleDeactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.DispatchRuleDeactivatedEvent), typeof(DispatchRuleDeactivatedEventSchema));

        sink.RegisterPayloadMapper("DispatchRuleRegisteredEvent", e =>
        {
            var evt = (DomainEvents.DispatchRuleRegisteredEvent)e;
            return new DispatchRuleRegisteredEventSchema(
                evt.DispatchRuleId.Value,
                evt.RuleName,
                evt.RouteRef,
                evt.Condition.ConditionType.Value,
                evt.Condition.MatchValue,
                evt.Priority);
        });
        sink.RegisterPayloadMapper("DispatchRuleDeactivatedEvent", e =>
        {
            var evt = (DomainEvents.DispatchRuleDeactivatedEvent)e;
            return new DispatchRuleDeactivatedEventSchema(evt.DispatchRuleId.Value);
        });
    }
}
