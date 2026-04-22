using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDecision;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDecision;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemPolicyPolicyDecisionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PolicyDecisionRecordedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyDecisionRecordedEvent), typeof(PolicyDecisionRecordedEventSchema));

        sink.RegisterPayloadMapper("PolicyDecisionRecordedEvent", e =>
        {
            var evt = (DomainEvents.PolicyDecisionRecordedEvent)e;
            return new PolicyDecisionRecordedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.PolicyDefinitionId,
                evt.SubjectId,
                evt.Action,
                evt.Resource,
                evt.Outcome.ToString(),
                evt.DecidedAt);
        });
    }
}
