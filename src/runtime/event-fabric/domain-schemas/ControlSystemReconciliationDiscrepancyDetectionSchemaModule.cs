using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyDetection;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemReconciliationDiscrepancyDetectionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DiscrepancyDetectedEvent", EventVersion.Default,
            typeof(DomainEvents.DiscrepancyDetectedEvent), typeof(DiscrepancyDetectedEventSchema));
        sink.RegisterSchema("DiscrepancyDetectionDismissedEvent", EventVersion.Default,
            typeof(DomainEvents.DiscrepancyDetectionDismissedEvent), typeof(DiscrepancyDetectionDismissedEventSchema));

        sink.RegisterPayloadMapper("DiscrepancyDetectedEvent", e =>
        {
            var evt = (DomainEvents.DiscrepancyDetectedEvent)e;
            return new DiscrepancyDetectedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Kind.ToString(),
                evt.SourceReference,
                evt.DetectedAt);
        });
        sink.RegisterPayloadMapper("DiscrepancyDetectionDismissedEvent", e =>
        {
            var evt = (DomainEvents.DiscrepancyDetectionDismissedEvent)e;
            return new DiscrepancyDetectionDismissedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Reason,
                evt.DismissedAt);
        });
    }
}
