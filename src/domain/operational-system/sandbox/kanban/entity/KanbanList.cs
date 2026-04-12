using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed class KanbanList : Entity
{
    private readonly List<KanbanCard> _cards = new();

    public KanbanListId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public KanbanPosition Position { get; private set; }
    public IReadOnlyList<KanbanCard> Cards => _cards.AsReadOnly();

    private KanbanList() { }

    internal static KanbanList Create(KanbanListId id, string name, KanbanPosition position)
    {
        return new KanbanList
        {
            Id = id,
            Name = name,
            Position = position
        };
    }

    internal void AddCard(KanbanCard card)
    {
        foreach (var existing in _cards)
        {
            if (existing.Position.Value >= card.Position.Value)
                existing.ShiftPosition(1);
        }

        _cards.Add(card);
    }

    internal void RemoveCard(KanbanCardId cardId)
    {
        var card = _cards.Find(c => c.Id == cardId);
        if (card is null) return;

        var removedPosition = card.Position.Value;
        _cards.Remove(card);

        foreach (var existing in _cards)
        {
            if (existing.Position.Value > removedPosition)
                existing.ShiftPosition(-1);
        }
    }

    internal void ReorderCard(KanbanCardId cardId, KanbanPosition newPosition)
    {
        var card = _cards.Find(c => c.Id == cardId);
        if (card is null) return;

        var oldPosition = card.Position.Value;
        var newPos = newPosition.Value;

        if (oldPosition < newPos)
        {
            foreach (var c in _cards)
            {
                if (c.Id != cardId && c.Position.Value > oldPosition && c.Position.Value <= newPos)
                    c.ShiftPosition(-1);
            }
        }
        else if (oldPosition > newPos)
        {
            foreach (var c in _cards)
            {
                if (c.Id != cardId && c.Position.Value >= newPos && c.Position.Value < oldPosition)
                    c.ShiftPosition(1);
            }
        }

        card.Reorder(newPosition);
    }

    internal KanbanCard? FindCard(KanbanCardId cardId)
    {
        return _cards.Find(c => c.Id == cardId);
    }
}
