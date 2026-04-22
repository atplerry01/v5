using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.OperationalSystem.Sandbox.Kanban;

public sealed class KanbanAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static KanbanBoardId NewBoardId(string seed) =>
        new(IdGen.Generate($"KanbanAggregateTests:{seed}:board"));

    private static KanbanListId NewListId(string seed) =>
        new(IdGen.Generate($"KanbanAggregateTests:{seed}:list"));

    private static KanbanCardId NewCardId(string seed) =>
        new(IdGen.Generate($"KanbanAggregateTests:{seed}:card"));

    private static DocumentRef DefaultDescription() =>
        new(new ContentId(IdGen.Generate("KanbanAggregateTests:description")));

    [Fact]
    public void Create_RaisesKanbanBoardCreatedEvent()
    {
        var id = NewBoardId("Create_Valid");

        var aggregate = KanbanAggregate.Create(id, "Sprint Board");

        var evt = Assert.IsType<KanbanBoardCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id.Value, evt.AggregateId.Value);
        Assert.Equal("Sprint Board", evt.Name);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewBoardId("Create_State");

        var aggregate = KanbanAggregate.Create(id, "My Board");

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("My Board", aggregate.Name);
        Assert.Empty(aggregate.Lists);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewBoardId("Stable");
        var b1 = KanbanAggregate.Create(id, "Board A");
        var b2 = KanbanAggregate.Create(id, "Board A");

        Assert.Equal(
            ((KanbanBoardCreatedEvent)b1.DomainEvents[0]).AggregateId.Value,
            ((KanbanBoardCreatedEvent)b2.DomainEvents[0]).AggregateId.Value);
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        var id = NewBoardId("Empty_Name");

        Assert.ThrowsAny<Exception>(() => KanbanAggregate.Create(id, "  "));
    }

    [Fact]
    public void CreateList_RaisesKanbanListCreatedEvent()
    {
        var id = NewBoardId("CreateList_Valid");
        var aggregate = KanbanAggregate.Create(id, "Board");
        aggregate.ClearDomainEvents();

        var listId = NewListId("CreateList_Valid");
        aggregate.CreateList(listId, "To Do", new KanbanPosition(0));

        var evt = Assert.IsType<KanbanListCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(listId, evt.ListId);
        Assert.Equal("To Do", evt.Name);
        Assert.Single(aggregate.Lists);
    }

    [Fact]
    public void CreateCard_RaisesKanbanCardCreatedEvent()
    {
        var boardId = NewBoardId("CreateCard_Valid");
        var listId = NewListId("CreateCard_Valid");
        var cardId = NewCardId("CreateCard_Valid");
        var aggregate = KanbanAggregate.Create(boardId, "Board");
        aggregate.CreateList(listId, "Backlog", new KanbanPosition(0));
        aggregate.ClearDomainEvents();

        aggregate.CreateCard(cardId, listId,
            new KanbanCardTitle("Implement feature X"),
            DefaultDescription(),
            new KanbanPosition(0));

        var evt = Assert.IsType<KanbanCardCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(cardId, evt.CardId);
        Assert.Equal(listId, evt.ListId);
    }

    [Fact]
    public void CompleteCard_RaisesKanbanCardCompletedEvent()
    {
        var boardId = NewBoardId("CompleteCard_Valid");
        var listId = NewListId("CompleteCard_Valid");
        var cardId = NewCardId("CompleteCard_Valid");
        var aggregate = KanbanAggregate.Create(boardId, "Board");
        aggregate.CreateList(listId, "In Progress", new KanbanPosition(0));
        aggregate.CreateCard(cardId, listId,
            new KanbanCardTitle("Task to complete"),
            DefaultDescription(),
            new KanbanPosition(0));
        aggregate.ClearDomainEvents();

        aggregate.CompleteCard(cardId);

        Assert.IsType<KanbanCardCompletedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void CompleteCard_AlreadyCompleted_Throws()
    {
        var boardId = NewBoardId("CompleteCard_Twice");
        var listId = NewListId("CompleteCard_Twice");
        var cardId = NewCardId("CompleteCard_Twice");
        var aggregate = KanbanAggregate.Create(boardId, "Board");
        aggregate.CreateList(listId, "Done", new KanbanPosition(0));
        aggregate.CreateCard(cardId, listId,
            new KanbanCardTitle("Already done"),
            DefaultDescription(),
            new KanbanPosition(0));
        aggregate.CompleteCard(cardId);

        Assert.ThrowsAny<Exception>(() => aggregate.CompleteCard(cardId));
    }

    [Fact]
    public void LoadFromHistory_RehydratesKanbanState()
    {
        var boardId = NewBoardId("History");

        var history = new object[]
        {
            new KanbanBoardCreatedEvent(new AggregateId(boardId.Value), "History Board")
        };

        var aggregate = (KanbanAggregate)Activator.CreateInstance(typeof(KanbanAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(boardId, aggregate.Id);
        Assert.Equal("History Board", aggregate.Name);
        Assert.Empty(aggregate.DomainEvents);
    }
}
