using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed class KanbanCard : Entity
{
    public KanbanCardId Id { get; private set; }
    public KanbanCardTitle Title { get; private set; }
    public DocumentRef Description { get; private set; }
    public KanbanListId ListId { get; private set; }
    public KanbanPosition Position { get; private set; }
    public bool IsCompleted { get; private set; }
    public KanbanPriority? Priority { get; private set; }

    private KanbanCard() { }

    internal static KanbanCard Create(
        KanbanCardId id,
        KanbanListId listId,
        KanbanCardTitle title,
        DocumentRef description,
        KanbanPosition position,
        KanbanPriority? priority)
    {
        return new KanbanCard
        {
            Id = id,
            ListId = listId,
            Title = title,
            Description = description,
            Position = position,
            IsCompleted = false,
            Priority = priority
        };
    }

    internal void MoveTo(KanbanListId newListId, KanbanPosition newPosition)
    {
        ListId = newListId;
        Position = newPosition;
    }

    internal void Reorder(KanbanPosition newPosition)
    {
        Position = newPosition;
    }

    internal void MarkCompleted()
    {
        IsCompleted = true;
    }

    internal void Update(KanbanCardTitle title, DocumentRef description)
    {
        Title = title;
        Description = description;
    }

    internal void ShiftPosition(int delta)
    {
        Position = new KanbanPosition(Position.Value + delta);
    }
}
