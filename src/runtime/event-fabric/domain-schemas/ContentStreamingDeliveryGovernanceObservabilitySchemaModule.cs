using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Observability;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingDeliveryGovernanceObservabilitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ObservabilityCapturedEvent", EventVersion.Default,
            typeof(DomainEvents.ObservabilityCapturedEvent), typeof(ObservabilityCapturedEventSchema));
        sink.RegisterSchema("ObservabilityUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.ObservabilityUpdatedEvent), typeof(ObservabilityUpdatedEventSchema));
        sink.RegisterSchema("ObservabilityFinalizedEvent", EventVersion.Default,
            typeof(DomainEvents.ObservabilityFinalizedEvent), typeof(ObservabilityFinalizedEventSchema));
        sink.RegisterSchema("ObservabilityArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.ObservabilityArchivedEvent), typeof(ObservabilityArchivedEventSchema));

        sink.RegisterPayloadMapper("ObservabilityCapturedEvent", e =>
        {
            var evt = (DomainEvents.ObservabilityCapturedEvent)e;
            return new ObservabilityCapturedEventSchema(
                evt.ObservabilityId.Value,
                evt.StreamRef.Value,
                evt.ArchiveRef?.Value,
                evt.Window.Start.Value,
                evt.Window.End.Value,
                evt.Snapshot.Viewers.Value,
                evt.Snapshot.Playbacks.Value,
                evt.Snapshot.Errors.Value,
                evt.Snapshot.Drops.Value,
                evt.Snapshot.AverageBitrate.BitsPerSecond,
                evt.Snapshot.AverageLatency.Milliseconds,
                evt.CapturedAt.Value);
        });
        sink.RegisterPayloadMapper("ObservabilityUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ObservabilityUpdatedEvent)e;
            return new ObservabilityUpdatedEventSchema(
                evt.ObservabilityId.Value,
                evt.PreviousSnapshot.Viewers.Value,
                evt.PreviousSnapshot.Playbacks.Value,
                evt.PreviousSnapshot.Errors.Value,
                evt.PreviousSnapshot.Drops.Value,
                evt.PreviousSnapshot.AverageBitrate.BitsPerSecond,
                evt.PreviousSnapshot.AverageLatency.Milliseconds,
                evt.NewSnapshot.Viewers.Value,
                evt.NewSnapshot.Playbacks.Value,
                evt.NewSnapshot.Errors.Value,
                evt.NewSnapshot.Drops.Value,
                evt.NewSnapshot.AverageBitrate.BitsPerSecond,
                evt.NewSnapshot.AverageLatency.Milliseconds,
                evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("ObservabilityFinalizedEvent", e =>
        {
            var evt = (DomainEvents.ObservabilityFinalizedEvent)e;
            return new ObservabilityFinalizedEventSchema(evt.ObservabilityId.Value, evt.FinalizedAt.Value);
        });
        sink.RegisterPayloadMapper("ObservabilityArchivedEvent", e =>
        {
            var evt = (DomainEvents.ObservabilityArchivedEvent)e;
            return new ObservabilityArchivedEventSchema(evt.ObservabilityId.Value, evt.ArchivedAt.Value);
        });
    }
}
