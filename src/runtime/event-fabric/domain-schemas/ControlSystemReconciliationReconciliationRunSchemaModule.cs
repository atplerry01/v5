using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ReconciliationRun;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemReconciliationReconciliationRunSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ReconciliationRunScheduledEvent", EventVersion.Default,
            typeof(DomainEvents.ReconciliationRunScheduledEvent), typeof(ReconciliationRunScheduledEventSchema));
        sink.RegisterSchema("ReconciliationRunStartedEvent", EventVersion.Default,
            typeof(DomainEvents.ReconciliationRunStartedEvent), typeof(ReconciliationRunStartedEventSchema));
        sink.RegisterSchema("ReconciliationRunCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.ReconciliationRunCompletedEvent), typeof(ReconciliationRunCompletedEventSchema));
        sink.RegisterSchema("ReconciliationRunAbortedEvent", EventVersion.Default,
            typeof(DomainEvents.ReconciliationRunAbortedEvent), typeof(ReconciliationRunAbortedEventSchema));

        sink.RegisterPayloadMapper("ReconciliationRunScheduledEvent", e =>
        {
            var evt = (DomainEvents.ReconciliationRunScheduledEvent)e;
            return new ReconciliationRunScheduledEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Scope);
        });
        sink.RegisterPayloadMapper("ReconciliationRunStartedEvent", e =>
        {
            var evt = (DomainEvents.ReconciliationRunStartedEvent)e;
            return new ReconciliationRunStartedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.StartedAt);
        });
        sink.RegisterPayloadMapper("ReconciliationRunCompletedEvent", e =>
        {
            var evt = (DomainEvents.ReconciliationRunCompletedEvent)e;
            return new ReconciliationRunCompletedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ChecksProcessed,
                evt.DiscrepanciesFound,
                evt.CompletedAt);
        });
        sink.RegisterPayloadMapper("ReconciliationRunAbortedEvent", e =>
        {
            var evt = (DomainEvents.ReconciliationRunAbortedEvent)e;
            return new ReconciliationRunAbortedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Reason,
                evt.AbortedAt);
        });
    }
}
