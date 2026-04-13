using Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;
using DomainEvents = Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whyce.Runtime.EventFabric.DomainSchemas;

public sealed class KanbanSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "KanbanBoardCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.KanbanBoardCreatedEvent),
            typeof(KanbanBoardCreatedEventSchema));

        sink.RegisterSchema(
            "KanbanListCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.KanbanListCreatedEvent),
            typeof(KanbanListCreatedEventSchema));

        sink.RegisterSchema(
            "KanbanCardCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.KanbanCardCreatedEvent),
            typeof(KanbanCardCreatedEventSchema));

        sink.RegisterSchema(
            "KanbanCardMovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.KanbanCardMovedEvent),
            typeof(KanbanCardMovedEventSchema));

        sink.RegisterSchema(
            "KanbanCardReorderedEvent",
            EventVersion.Default,
            typeof(DomainEvents.KanbanCardReorderedEvent),
            typeof(KanbanCardReorderedEventSchema));

        sink.RegisterSchema(
            "KanbanCardCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.KanbanCardCompletedEvent),
            typeof(KanbanCardCompletedEventSchema));

        sink.RegisterSchema(
            "KanbanCardDetailRevisedEvent",
            EventVersion.Default,
            typeof(DomainEvents.KanbanCardDetailRevisedEvent),
            typeof(KanbanCardUpdatedEventSchema));

        sink.RegisterPayloadMapper("KanbanBoardCreatedEvent", e =>
        {
            var evt = (DomainEvents.KanbanBoardCreatedEvent)e;
            return new KanbanBoardCreatedEventSchema(evt.AggregateId.Value, evt.Name);
        });

        sink.RegisterPayloadMapper("KanbanListCreatedEvent", e =>
        {
            var evt = (DomainEvents.KanbanListCreatedEvent)e;
            return new KanbanListCreatedEventSchema(evt.AggregateId.Value, evt.ListId.Value, evt.Name, evt.Position.Value);
        });

        sink.RegisterPayloadMapper("KanbanCardCreatedEvent", e =>
        {
            var evt = (DomainEvents.KanbanCardCreatedEvent)e;
            return new KanbanCardCreatedEventSchema(
                evt.AggregateId.Value, evt.CardId.Value, evt.ListId.Value,
                evt.Title, evt.Description, evt.Position.Value, evt.Priority?.ToString());
        });

        sink.RegisterPayloadMapper("KanbanCardMovedEvent", e =>
        {
            var evt = (DomainEvents.KanbanCardMovedEvent)e;
            return new KanbanCardMovedEventSchema(
                evt.AggregateId.Value, evt.CardId.Value, evt.FromListId.Value, evt.ToListId.Value, evt.NewPosition.Value);
        });

        sink.RegisterPayloadMapper("KanbanCardReorderedEvent", e =>
        {
            var evt = (DomainEvents.KanbanCardReorderedEvent)e;
            return new KanbanCardReorderedEventSchema(
                evt.AggregateId.Value, evt.CardId.Value, evt.ListId.Value, evt.NewPosition.Value);
        });

        sink.RegisterPayloadMapper("KanbanCardCompletedEvent", e =>
        {
            var evt = (DomainEvents.KanbanCardCompletedEvent)e;
            return new KanbanCardCompletedEventSchema(evt.AggregateId.Value, evt.CardId.Value);
        });

        sink.RegisterPayloadMapper("KanbanCardDetailRevisedEvent", e =>
        {
            var evt = (DomainEvents.KanbanCardDetailRevisedEvent)e;
            return new KanbanCardUpdatedEventSchema(
                evt.AggregateId.Value, evt.CardId.Value, evt.Title, evt.Description);
        });
    }
}
