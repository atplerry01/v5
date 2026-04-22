using Whycespace.Shared.Contracts.Events.Control.Scheduling.ExecutionControl;
using DomainEvents = Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSchedulingExecutionControlSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ExecutionControlSignalIssuedEvent", EventVersion.Default,
            typeof(DomainEvents.ExecutionControlSignalIssuedEvent), typeof(ExecutionControlSignalIssuedEventSchema));
        sink.RegisterSchema("ExecutionControlSignalOutcomeRecordedEvent", EventVersion.Default,
            typeof(DomainEvents.ExecutionControlSignalOutcomeRecordedEvent), typeof(ExecutionControlSignalOutcomeRecordedEventSchema));

        sink.RegisterPayloadMapper("ExecutionControlSignalIssuedEvent", e =>
        {
            var evt = (DomainEvents.ExecutionControlSignalIssuedEvent)e;
            return new ExecutionControlSignalIssuedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.JobInstanceId,
                evt.Signal.ToString(),
                evt.ActorId,
                evt.IssuedAt);
        });
        sink.RegisterPayloadMapper("ExecutionControlSignalOutcomeRecordedEvent", e =>
        {
            var evt = (DomainEvents.ExecutionControlSignalOutcomeRecordedEvent)e;
            return new ExecutionControlSignalOutcomeRecordedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Outcome.ToString());
        });
    }
}
