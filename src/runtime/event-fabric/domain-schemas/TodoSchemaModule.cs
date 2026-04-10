using Whyce.Shared.Contracts.Events.Todo;
using DomainEvents = Whycespace.Domain.Operational.Sandbox.Todo;

namespace Whyce.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the operational/sandbox/todo domain.
///
/// Owns the binding from Todo domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events into shared schema records for the projection layer.
/// Relocated from <c>src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs</c>
/// under Phase 1.5 §5.1.2 BPV-D01 to remove typed domain references from host.
/// </summary>
public sealed class TodoSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "TodoCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.TodoCreatedEvent),
            typeof(TodoCreatedEventSchema));

        sink.RegisterSchema(
            "TodoUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.TodoUpdatedEvent),
            typeof(TodoUpdatedEventSchema));

        sink.RegisterSchema(
            "TodoCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.TodoCompletedEvent),
            typeof(TodoCompletedEventSchema));

        sink.RegisterPayloadMapper("TodoCreatedEvent", e =>
        {
            var evt = (DomainEvents.TodoCreatedEvent)e;
            return new TodoCreatedEventSchema(evt.AggregateId.Value, evt.Title);
        });
        sink.RegisterPayloadMapper("TodoUpdatedEvent", e =>
        {
            var evt = (DomainEvents.TodoUpdatedEvent)e;
            return new TodoUpdatedEventSchema(evt.AggregateId.Value, evt.Title);
        });
        sink.RegisterPayloadMapper("TodoCompletedEvent", e =>
        {
            var evt = (DomainEvents.TodoCompletedEvent)e;
            return new TodoCompletedEventSchema(evt.AggregateId.Value);
        });
    }
}
