using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEnforcement;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEnforcement;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemPolicyPolicyEnforcementSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PolicyEnforcedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyEnforcedEvent), typeof(PolicyEnforcedEventSchema));

        sink.RegisterPayloadMapper("PolicyEnforcedEvent", e =>
        {
            var evt = (DomainEvents.PolicyEnforcedEvent)e;
            return new PolicyEnforcedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.PolicyDecisionId,
                evt.TargetId,
                evt.Outcome.ToString(),
                evt.EnforcedAt,
                evt.IsNoPolicyFlagAnomaly);
        });
    }
}
