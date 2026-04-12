using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed class KanbanCard : Entity
{
    public KanbanCardId Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public KanbanListId ListId { get; private set; }
    public KanbanPosition Position { get; private set; }
    public bool IsCompleted { get; private set; }
    public KanbanPriority? Priority { get; private set; }

    private KanbanCard() { }

    internal static KanbanCard Create(
        KanbanCardId id,
        KanbanListId listId,
        string title,
        string description,
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

    internal void Update(string title, string description)
    {
        Title = title;
        Description = description;
    }

    internal void ShiftPosition(int delta)
    {
        Position = new KanbanPosition(Position.Value + delta);
    }
}
