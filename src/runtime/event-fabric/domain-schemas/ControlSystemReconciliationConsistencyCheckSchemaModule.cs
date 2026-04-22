using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ConsistencyCheck;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemReconciliationConsistencyCheckSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ConsistencyCheckInitiatedEvent", EventVersion.Default,
            typeof(DomainEvents.ConsistencyCheckInitiatedEvent), typeof(ConsistencyCheckInitiatedEventSchema));
        sink.RegisterSchema("ConsistencyCheckCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.ConsistencyCheckCompletedEvent), typeof(ConsistencyCheckCompletedEventSchema));

        sink.RegisterPayloadMapper("ConsistencyCheckInitiatedEvent", e =>
        {
            var evt = (DomainEvents.ConsistencyCheckInitiatedEvent)e;
            return new ConsistencyCheckInitiatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ScopeTarget,
                evt.InitiatedAt);
        });
        sink.RegisterPayloadMapper("ConsistencyCheckCompletedEvent", e =>
        {
            var evt = (DomainEvents.ConsistencyCheckCompletedEvent)e;
            return new ConsistencyCheckCompletedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.HasDiscrepancies,
                evt.CompletedAt);
        });
    }
}
