using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.List;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Workflow;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Operational.Sandbox.Kanban;

// WP-1: All operational endpoints require authenticated identity.
[Authorize]
[ApiController]
[Route("api/kanban")]
[ApiExplorerSettings(GroupName = "operational.sandbox.kanban")]
public sealed class KanbanController : ControllerBase
{
    private static readonly DomainRoute KanbanRoute = new("operational", "sandbox", "kanban");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IWorkflowDispatcher _workflowDispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public KanbanController(
        ISystemIntentDispatcher dispatcher,
        IWorkflowDispatcher workflowDispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _workflowDispatcher = workflowDispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("board/create")]
    public async Task<IActionResult> CreateBoard([FromBody] ApiRequest<CreateBoardRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var boardId = _idGenerator.Generate($"kanban:board:{payload.UserId}:{payload.Name}");
        var cmd = new CreateKanbanBoardCommand(boardId, payload.Name, boardId);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CreateBoardResponseModel(boardId), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.board_create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("list/create")]
    public async Task<IActionResult> CreateList([FromBody] ApiRequest<CreateListRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var listId = _idGenerator.Generate($"kanban:list:{payload.BoardId}:{payload.Name}");
        var cmd = new CreateKanbanListCommand(payload.BoardId, listId, payload.Name, payload.Position);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CreateListResponseModel(listId, payload.BoardId), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.list_create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("card/create")]
    public async Task<IActionResult> CreateCard([FromBody] ApiRequest<CreateCardRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var cardId = _idGenerator.Generate($"kanban:card:{payload.BoardId}:{payload.ListId}:{payload.Title}");
        var cmd = new CreateKanbanCardCommand(payload.BoardId, cardId, payload.ListId, payload.Title, payload.Description, payload.Position);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CreateCardResponseModel(cardId, payload.BoardId), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.card_create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("card/move")]
    public async Task<IActionResult> MoveCard([FromBody] ApiRequest<MoveCardRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var cmd = new MoveKanbanCardCommand(payload.BoardId, payload.CardId, payload.FromListId, payload.ToListId, payload.NewPosition);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("card_moved"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.card_move_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("card/reorder")]
    public async Task<IActionResult> ReorderCard([FromBody] ApiRequest<ReorderCardRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var cmd = new ReorderKanbanCardCommand(payload.BoardId, payload.CardId, payload.ListId, payload.NewPosition);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("card_reordered"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.card_reorder_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("card/complete")]
    public async Task<IActionResult> CompleteCard([FromBody] ApiRequest<CompleteCardRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var cmd = new CompleteKanbanCardCommand(payload.BoardId, payload.CardId);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("card_completed"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.card_complete_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("card/update")]
    public async Task<IActionResult> UpdateCard([FromBody] ApiRequest<UpdateCardRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var cmd = new UpdateKanbanCardCommand(payload.BoardId, payload.CardId, payload.Title, payload.Description);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("card_updated"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.card_update_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpPost("card/approval/start")]
    public async Task<IActionResult> StartCardApproval([FromBody] ApiRequest<StartCardApprovalRequestModel> request, CancellationToken cancellationToken)
    {
        var payload = request.Data;
        var intent = new CardApprovalIntent(
            payload.BoardId,
            payload.CardId,
            payload.FromListId,
            payload.ReviewListId,
            payload.UserId);
        var result = await _workflowDispatcher.StartWorkflowAsync(
            CardApprovalWorkflowNames.Approve, intent, KanbanRoute);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("card_approval_started"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "operational.sandbox.kanban.card_approval_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpGet("{boardId:guid}")]
    public async Task<IActionResult> GetBoard(Guid boardId, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT state
            FROM projection_operational_sandbox_kanban.kanban_read_model
            WHERE aggregate_id = @id
            LIMIT 1
            """, conn);
        cmd.Parameters.AddWithValue("id", boardId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return NotFound(ApiResponse.Fail(
                "operational.sandbox.kanban.board_not_found",
                $"Board {boardId} not found.",
                _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var state = JsonSerializer.Deserialize<KanbanBoardReadModel>(stateJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return Ok(ApiResponse.Ok(state!, this.RequestCorrelationId(), _clock.UtcNow));
    }
}

public sealed record CreateBoardRequestModel(string Name, string UserId);
public sealed record CreateListRequestModel(Guid BoardId, string Name, int Position);
public sealed record CreateCardRequestModel(Guid BoardId, Guid ListId, string Title, string Description, int Position);
public sealed record MoveCardRequestModel(Guid BoardId, Guid CardId, Guid FromListId, Guid ToListId, int NewPosition);
public sealed record ReorderCardRequestModel(Guid BoardId, Guid CardId, Guid ListId, int NewPosition);
public sealed record CompleteCardRequestModel(Guid BoardId, Guid CardId);
public sealed record UpdateCardRequestModel(Guid BoardId, Guid CardId, string Title, string Description);

public sealed record StartCardApprovalRequestModel(Guid BoardId, Guid CardId, Guid FromListId, Guid ReviewListId, string UserId);

public sealed record CreateBoardResponseModel(Guid BoardId);
public sealed record CreateListResponseModel(Guid ListId, Guid BoardId);
public sealed record CreateCardResponseModel(Guid CardId, Guid BoardId);
