using Whycespace.Shared.Contracts.Events.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Events.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Events.Operational.Sandbox.Kanban.List;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.List;

namespace Whycespace.Projections.Operational.Sandbox.Kanban.Reducer;

/// <summary>
/// Pure state reducer for the Kanban board read model.
/// Each Apply method takes the current state + an event and returns the new state.
/// No I/O, no side effects — transformation logic only.
/// </summary>
public static class KanbanProjectionReducer
{
    public static KanbanBoardReadModel Apply(KanbanBoardReadModel state, KanbanBoardCreatedEventSchema e)
    {
        return state with { Name = e.Name };
    }

    public static KanbanBoardReadModel Apply(KanbanBoardReadModel state, KanbanListCreatedEventSchema e)
    {
        var lists = new List<KanbanListReadModel>(state.Lists);
        if (lists.Exists(l => l.ListId == e.ListId)) return state;

        lists.Add(new KanbanListReadModel
        {
            ListId = e.ListId,
            Name = e.Name,
            Position = e.Position
        });
        lists.Sort((a, b) => a.Position.CompareTo(b.Position));
        return state with { Lists = lists };
    }

    public static KanbanBoardReadModel Apply(KanbanBoardReadModel state, KanbanCardCreatedEventSchema e)
    {
        var lists = new List<KanbanListReadModel>(state.Lists);
        var listIndex = lists.FindIndex(l => l.ListId == e.ListId);
        if (listIndex < 0) return state;

        var list = lists[listIndex];
        var cards = new List<KanbanCardReadModel>(list.Cards);
        if (cards.Exists(c => c.CardId == e.CardId)) return state;

        cards.Add(new KanbanCardReadModel
        {
            CardId = e.CardId,
            Title = e.Title,
            Description = e.Description,
            Position = e.Position,
            IsCompleted = false
        });
        cards.Sort((a, b) => a.Position.CompareTo(b.Position));

        lists[listIndex] = list with { Cards = cards };
        return state with { Lists = lists };
    }

    public static KanbanBoardReadModel? Apply(KanbanBoardReadModel? state, KanbanCardMovedEventSchema e)
    {
        if (state is null) return null;

        var lists = new List<KanbanListReadModel>(state.Lists);
        var fromIndex = lists.FindIndex(l => l.ListId == e.FromListId);
        var toIndex = lists.FindIndex(l => l.ListId == e.ToListId);
        if (fromIndex < 0 || toIndex < 0) return state;

        var fromCards = new List<KanbanCardReadModel>(lists[fromIndex].Cards);
        var cardIndex = fromCards.FindIndex(c => c.CardId == e.CardId);
        if (cardIndex < 0) return state;

        var card = fromCards[cardIndex] with { Position = e.NewPosition };
        fromCards.RemoveAt(cardIndex);
        lists[fromIndex] = lists[fromIndex] with { Cards = fromCards };

        var toCards = new List<KanbanCardReadModel>(lists[toIndex].Cards);
        toCards.Add(card);
        toCards.Sort((a, b) => a.Position.CompareTo(b.Position));
        lists[toIndex] = lists[toIndex] with { Cards = toCards };

        return state with { Lists = lists };
    }

    public static KanbanBoardReadModel? Apply(KanbanBoardReadModel? state, KanbanCardReorderedEventSchema e)
    {
        if (state is null) return null;

        var lists = new List<KanbanListReadModel>(state.Lists);
        var listIndex = lists.FindIndex(l => l.ListId == e.ListId);
        if (listIndex < 0) return state;

        var cards = new List<KanbanCardReadModel>(lists[listIndex].Cards);
        var cardIndex = cards.FindIndex(c => c.CardId == e.CardId);
        if (cardIndex < 0) return state;

        cards[cardIndex] = cards[cardIndex] with { Position = e.NewPosition };
        cards.Sort((a, b) => a.Position.CompareTo(b.Position));
        lists[listIndex] = lists[listIndex] with { Cards = cards };

        return state with { Lists = lists };
    }

    public static KanbanBoardReadModel? Apply(KanbanBoardReadModel? state, KanbanCardCompletedEventSchema e)
    {
        if (state is null) return null;

        var lists = new List<KanbanListReadModel>(state.Lists);
        for (var i = 0; i < lists.Count; i++)
        {
            var cards = new List<KanbanCardReadModel>(lists[i].Cards);
            var cardIndex = cards.FindIndex(c => c.CardId == e.CardId);
            if (cardIndex >= 0)
            {
                cards[cardIndex] = cards[cardIndex] with { IsCompleted = true };
                lists[i] = lists[i] with { Cards = cards };
                return state with { Lists = lists };
            }
        }

        return state;
    }

    public static KanbanBoardReadModel? Apply(KanbanBoardReadModel? state, KanbanCardUpdatedEventSchema e)
    {
        if (state is null) return null;

        var lists = new List<KanbanListReadModel>(state.Lists);
        for (var i = 0; i < lists.Count; i++)
        {
            var cards = new List<KanbanCardReadModel>(lists[i].Cards);
            var cardIndex = cards.FindIndex(c => c.CardId == e.CardId);
            if (cardIndex >= 0)
            {
                cards[cardIndex] = cards[cardIndex] with { Title = e.Title, Description = e.Description };
                lists[i] = lists[i] with { Cards = cards };
                return state with { Lists = lists };
            }
        }

        return state;
    }
}
