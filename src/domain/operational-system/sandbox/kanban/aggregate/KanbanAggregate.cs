using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public sealed class KanbanAggregate : AggregateRoot
{
    private readonly List<KanbanList> _lists = new();

    public KanbanBoardId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IReadOnlyList<KanbanList> Lists => _lists.AsReadOnly();

    private KanbanAggregate() { }

    public static KanbanAggregate Create(KanbanBoardId id, string name)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), KanbanDomainErrors.BoardNameRequired);

        var aggregate = new KanbanAggregate { Id = id };
        aggregate.RaiseDomainEvent(new KanbanBoardCreatedEvent(new AggregateId(id.Value), name));
        return aggregate;
    }

    public void CreateList(KanbanListId listId, string name, KanbanPosition position)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), KanbanDomainErrors.ListNameRequired);
        Guard.Against(position.Value < 0, KanbanDomainErrors.InvalidPosition);
        Guard.Against(_lists.Exists(l => l.Id == listId), KanbanDomainErrors.DuplicateListId);
        Guard.Against(_lists.Exists(l => l.Position == position), KanbanDomainErrors.DuplicateListPosition);

        RaiseDomainEvent(new KanbanListCreatedEvent(new AggregateId(Id.Value), listId, name, position));
    }

    public void CreateCard(
        KanbanCardId cardId,
        KanbanListId listId,
        string title,
        string description,
        KanbanPosition position,
        KanbanPriority? priority = null)
    {
        Guard.Against(string.IsNullOrWhiteSpace(title), KanbanDomainErrors.CardTitleRequired);
        Guard.Against(position.Value < 0, KanbanDomainErrors.InvalidPosition);
        Guard.Against(!_lists.Exists(l => l.Id == listId), KanbanDomainErrors.ListNotFound);
        Guard.Against(CardExistsOnBoard(cardId), KanbanDomainErrors.DuplicateCardId);

        RaiseDomainEvent(new KanbanCardCreatedEvent(
            new AggregateId(Id.Value), cardId, listId, title, description, position, priority));
    }

    public void MoveCard(KanbanCardId cardId, KanbanListId newListId, KanbanPosition newPosition)
    {
        Guard.Against(newPosition.Value < 0, KanbanDomainErrors.InvalidPosition);
        Guard.Against(!_lists.Exists(l => l.Id == newListId), KanbanDomainErrors.ListNotFound);

        var (sourceList, card) = FindCardOnBoard(cardId);
        Guard.Against(card is null || sourceList is null, KanbanDomainErrors.CardNotFound);
        Guard.Against(card!.IsCompleted, KanbanDomainErrors.CardAlreadyCompleted);
        Guard.Against(sourceList!.Id == newListId, KanbanDomainErrors.InvalidMove);

        RaiseDomainEvent(new KanbanCardMovedEvent(
            new AggregateId(Id.Value), cardId, sourceList.Id, newListId, newPosition));
    }

    public void ReorderCard(KanbanCardId cardId, KanbanPosition newPosition)
    {
        Guard.Against(newPosition.Value < 0, KanbanDomainErrors.InvalidPosition);

        var (sourceList, card) = FindCardOnBoard(cardId);
        Guard.Against(card is null || sourceList is null, KanbanDomainErrors.CardNotFound);
        Guard.Against(card!.IsCompleted, KanbanDomainErrors.CardAlreadyCompleted);

        RaiseDomainEvent(new KanbanCardReorderedEvent(
            new AggregateId(Id.Value), cardId, sourceList!.Id, newPosition));
    }

    public void CompleteCard(KanbanCardId cardId)
    {
        var (_, card) = FindCardOnBoard(cardId);
        Guard.Against(card is null, KanbanDomainErrors.CardNotFound);
        Guard.Against(card!.IsCompleted, KanbanDomainErrors.CardAlreadyCompleted);

        RaiseDomainEvent(new KanbanCardCompletedEvent(new AggregateId(Id.Value), cardId));
    }

    public void ReviseCardDetail(KanbanCardId cardId, string title, string description)
    {
        Guard.Against(string.IsNullOrWhiteSpace(title), KanbanDomainErrors.CardTitleRequired);

        var (_, card) = FindCardOnBoard(cardId);
        Guard.Against(card is null, KanbanDomainErrors.CardNotFound);
        Guard.Against(card!.IsCompleted, KanbanDomainErrors.CardAlreadyCompleted);

        RaiseDomainEvent(new KanbanCardDetailRevisedEvent(
            new AggregateId(Id.Value), cardId, title, description));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case KanbanBoardCreatedEvent e:
                Id = new KanbanBoardId(e.AggregateId.Value);
                Name = e.Name;
                break;

            case KanbanListCreatedEvent e:
                _lists.Add(KanbanList.Create(e.ListId, e.Name, e.Position));
                break;

            case KanbanCardCreatedEvent e:
            {
                var list = _lists.Find(l => l.Id == e.ListId)!;
                var card = KanbanCard.Create(e.CardId, e.ListId, e.Title, e.Description, e.Position, e.Priority);
                list.AddCard(card);
                break;
            }

            case KanbanCardMovedEvent e:
            {
                var fromList = _lists.Find(l => l.Id == e.FromListId)!;
                var toList = _lists.Find(l => l.Id == e.ToListId)!;
                var card = fromList.FindCard(e.CardId)!;

                fromList.RemoveCard(e.CardId);
                card.MoveTo(e.ToListId, e.NewPosition);
                toList.AddCard(card);
                break;
            }

            case KanbanCardReorderedEvent e:
            {
                var list = _lists.Find(l => l.Id == e.ListId)!;
                list.ReorderCard(e.CardId, e.NewPosition);
                break;
            }

            case KanbanCardDetailRevisedEvent e:
            {
                var (_, card) = FindCardOnBoardInternal(e.CardId);
                card?.Update(e.Title, e.Description);
                break;
            }

            case KanbanCardCompletedEvent e:
            {
                var (_, card) = FindCardOnBoardInternal(e.CardId);
                card?.MarkCompleted();
                break;
            }
        }
    }

    protected override void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private bool CardExistsOnBoard(KanbanCardId cardId)
    {
        return _lists.Exists(l => l.FindCard(cardId) is not null);
    }

    private (KanbanList? List, KanbanCard? Card) FindCardOnBoard(KanbanCardId cardId)
    {
        return FindCardOnBoardInternal(cardId);
    }

    private (KanbanList? List, KanbanCard? Card) FindCardOnBoardInternal(KanbanCardId cardId)
    {
        foreach (var list in _lists)
        {
            var card = list.FindCard(cardId);
            if (card is not null)
                return (list, card);
        }

        return (null, null);
    }
}
