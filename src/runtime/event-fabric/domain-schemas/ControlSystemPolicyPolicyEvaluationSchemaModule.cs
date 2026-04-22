using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEvaluation;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemPolicyPolicyEvaluationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PolicyEvaluationRecordedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyEvaluationRecordedEvent), typeof(PolicyEvaluationRecordedEventSchema));
        sink.RegisterSchema("PolicyEvaluationVerdictIssuedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyEvaluationVerdictIssuedEvent), typeof(PolicyEvaluationVerdictIssuedEventSchema));

        sink.RegisterPayloadMapper("PolicyEvaluationRecordedEvent", e =>
        {
            var evt = (DomainEvents.PolicyEvaluationRecordedEvent)e;
            return new PolicyEvaluationRecordedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.PolicyId,
                evt.ActorId,
                evt.Action,
                evt.CorrelationId);
        });
        sink.RegisterPayloadMapper("PolicyEvaluationVerdictIssuedEvent", e =>
        {
            var evt = (DomainEvents.PolicyEvaluationVerdictIssuedEvent)e;
            return new PolicyEvaluationVerdictIssuedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Outcome.ToString(),
                evt.DecisionHash);
        });
    }
}
