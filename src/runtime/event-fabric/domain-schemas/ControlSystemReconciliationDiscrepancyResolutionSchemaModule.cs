using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyResolution;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemReconciliationDiscrepancyResolutionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DiscrepancyResolutionInitiatedEvent", EventVersion.Default,
            typeof(DomainEvents.DiscrepancyResolutionInitiatedEvent), typeof(DiscrepancyResolutionInitiatedEventSchema));
        sink.RegisterSchema("DiscrepancyResolutionCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.DiscrepancyResolutionCompletedEvent), typeof(DiscrepancyResolutionCompletedEventSchema));

        sink.RegisterPayloadMapper("DiscrepancyResolutionInitiatedEvent", e =>
        {
            var evt = (DomainEvents.DiscrepancyResolutionInitiatedEvent)e;
            return new DiscrepancyResolutionInitiatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.DetectionId.Value,
                evt.InitiatedAt);
        });
        sink.RegisterPayloadMapper("DiscrepancyResolutionCompletedEvent", e =>
        {
            var evt = (DomainEvents.DiscrepancyResolutionCompletedEvent)e;
            return new DiscrepancyResolutionCompletedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Outcome.ToString(),
                evt.Notes,
                evt.CompletedAt);
        });
    }
}
