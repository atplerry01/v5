using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.List;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Systems.Downstream.Operational.Sandbox.Kanban;

public sealed class KanbanIntentHandler : IKanbanIntentHandler
{
    private static readonly DomainRoute KanbanRoute = new("operational", "sandbox", "kanban");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;

    public KanbanIntentHandler(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
    }

    public async Task<KanbanSystemResult> HandleCreateBoardAsync(CreateKanbanBoardIntent intent)
    {
        var boardId = _idGenerator.Generate($"kanban:board:{intent.UserId}:{intent.Name}");
        var cmd = new CreateKanbanBoardCommand(boardId, intent.Name, boardId);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute);
        return result.IsSuccess
            ? KanbanSystemResult.Ok(boardId, "board_created")
            : KanbanSystemResult.Fail(result.Error ?? "Unknown error");
    }

    public async Task<KanbanSystemResult> HandleCreateListAsync(CreateKanbanListIntent intent)
    {
        var listId = _idGenerator.Generate($"kanban:list:{intent.BoardId}:{intent.Name}");
        var cmd = new CreateKanbanListCommand(intent.BoardId, listId, intent.Name, intent.Position);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute);
        return result.IsSuccess
            ? KanbanSystemResult.Ok(intent.BoardId, "list_created")
            : KanbanSystemResult.Fail(result.Error ?? "Unknown error");
    }

    public async Task<KanbanSystemResult> HandleCreateCardAsync(CreateKanbanCardIntent intent)
    {
        var cardId = _idGenerator.Generate($"kanban:card:{intent.BoardId}:{intent.ListId}:{intent.Title}");
        var cmd = new CreateKanbanCardCommand(intent.BoardId, cardId, intent.ListId, intent.Title, intent.Description, intent.Position);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute);
        return result.IsSuccess
            ? KanbanSystemResult.Ok(intent.BoardId, "card_created")
            : KanbanSystemResult.Fail(result.Error ?? "Unknown error");
    }

    public async Task<KanbanSystemResult> HandleMoveCardAsync(MoveKanbanCardIntent intent)
    {
        var cmd = new MoveKanbanCardCommand(intent.BoardId, intent.CardId, intent.FromListId, intent.ToListId, intent.NewPosition);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute);
        return result.IsSuccess
            ? KanbanSystemResult.Ok(intent.BoardId, "card_moved")
            : KanbanSystemResult.Fail(result.Error ?? "Unknown error");
    }

    public async Task<KanbanSystemResult> HandleReorderCardAsync(ReorderKanbanCardIntent intent)
    {
        var cmd = new ReorderKanbanCardCommand(intent.BoardId, intent.CardId, intent.ListId, intent.NewPosition);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute);
        return result.IsSuccess
            ? KanbanSystemResult.Ok(intent.BoardId, "card_reordered")
            : KanbanSystemResult.Fail(result.Error ?? "Unknown error");
    }

    public async Task<KanbanSystemResult> HandleCompleteCardAsync(CompleteKanbanCardIntent intent)
    {
        var cmd = new CompleteKanbanCardCommand(intent.BoardId, intent.CardId);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute);
        return result.IsSuccess
            ? KanbanSystemResult.Ok(intent.BoardId, "card_completed")
            : KanbanSystemResult.Fail(result.Error ?? "Unknown error");
    }

    public async Task<KanbanSystemResult> HandleUpdateCardAsync(UpdateKanbanCardIntent intent)
    {
        var cmd = new UpdateKanbanCardCommand(intent.BoardId, intent.CardId, intent.Title, intent.Description);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute);
        return result.IsSuccess
            ? KanbanSystemResult.Ok(intent.BoardId, "card_updated")
            : KanbanSystemResult.Fail(result.Error ?? "Unknown error");
    }
}
