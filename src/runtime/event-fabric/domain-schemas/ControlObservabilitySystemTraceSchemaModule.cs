using Whycespace.Shared.Contracts.Events.Control.Observability.SystemTrace;
using DomainEvents = Whycespace.Domain.ControlSystem.Observability.SystemTrace;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlObservabilitySystemTraceSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SystemTraceSpanStartedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemTraceSpanStartedEvent), typeof(SystemTraceSpanStartedEventSchema));
        sink.RegisterSchema("SystemTraceSpanCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemTraceSpanCompletedEvent), typeof(SystemTraceSpanCompletedEventSchema));

        sink.RegisterPayloadMapper("SystemTraceSpanStartedEvent", e =>
        {
            var evt = (DomainEvents.SystemTraceSpanStartedEvent)e;
            return new SystemTraceSpanStartedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.TraceId,
                evt.OperationName,
                evt.StartedAt,
                evt.ParentSpanId);
        });
        sink.RegisterPayloadMapper("SystemTraceSpanCompletedEvent", e =>
        {
            var evt = (DomainEvents.SystemTraceSpanCompletedEvent)e;
            return new SystemTraceSpanCompletedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.CompletedAt,
                evt.Status.ToString());
        });
    }
}
