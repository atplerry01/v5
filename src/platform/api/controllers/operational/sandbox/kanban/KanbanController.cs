using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Api.Controllers.Operational.Sandbox.Kanban;

[ApiController]
[Route("api/kanban")]
[ApiExplorerSettings(GroupName = "operational.sandbox.kanban")]
public sealed class KanbanController : ControllerBase
{
    private static readonly DomainRoute KanbanRoute = new("operational", "sandbox", "kanban");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly string _projectionsConnectionString;

    public KanbanController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("board/create")]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var boardId = _idGenerator.Generate($"kanban:board:{request.UserId}:{request.Name}");
        var cmd = new CreateKanbanBoardCommand(boardId, request.Name, boardId);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(new { status = "board_created", boardId, correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("list/create")]
    public async Task<IActionResult> CreateList([FromBody] CreateListRequest request, CancellationToken cancellationToken)
    {
        var listId = _idGenerator.Generate($"kanban:list:{request.BoardId}:{request.Name}");
        var cmd = new CreateKanbanListCommand(request.BoardId, listId, request.Name, request.Position);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(new { status = "list_created", listId, boardId = request.BoardId, correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("card/create")]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request, CancellationToken cancellationToken)
    {
        var cardId = _idGenerator.Generate($"kanban:card:{request.BoardId}:{request.ListId}:{request.Title}");
        var cmd = new CreateKanbanCardCommand(request.BoardId, cardId, request.ListId, request.Title, request.Description, request.Position);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(new { status = "card_created", cardId, boardId = request.BoardId, correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("card/move")]
    public async Task<IActionResult> MoveCard([FromBody] MoveCardRequest request, CancellationToken cancellationToken)
    {
        var cmd = new MoveKanbanCardCommand(request.BoardId, request.CardId, request.FromListId, request.ToListId, request.NewPosition);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(new { status = "card_moved", correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("card/reorder")]
    public async Task<IActionResult> ReorderCard([FromBody] ReorderCardRequest request, CancellationToken cancellationToken)
    {
        var cmd = new ReorderKanbanCardCommand(request.BoardId, request.CardId, request.ListId, request.NewPosition);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(new { status = "card_reordered", correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("card/complete")]
    public async Task<IActionResult> CompleteCard([FromBody] CompleteCardRequest request, CancellationToken cancellationToken)
    {
        var cmd = new CompleteKanbanCardCommand(request.BoardId, request.CardId);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(new { status = "card_completed", correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("card/update")]
    public async Task<IActionResult> UpdateCard([FromBody] UpdateCardRequest request, CancellationToken cancellationToken)
    {
        var cmd = new UpdateKanbanCardCommand(request.BoardId, request.CardId, request.Title, request.Description);
        var result = await _dispatcher.DispatchAsync(cmd, KanbanRoute, cancellationToken);
        return result.IsSuccess
            ? Ok(new { status = "card_updated", correlationId = result.CorrelationId })
            : BadRequest(new { error = result.Error });
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
            return NotFound();

        var stateJson = reader.GetString(0);
        var state = JsonSerializer.Deserialize<KanbanBoardDto>(stateJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return Ok(state);
    }
}

public sealed record CreateBoardRequest(string Name, string UserId);
public sealed record CreateListRequest(Guid BoardId, string Name, int Position);
public sealed record CreateCardRequest(Guid BoardId, Guid ListId, string Title, string Description, int Position);
public sealed record MoveCardRequest(Guid BoardId, Guid CardId, Guid FromListId, Guid ToListId, int NewPosition);
public sealed record ReorderCardRequest(Guid BoardId, Guid CardId, Guid ListId, int NewPosition);
public sealed record CompleteCardRequest(Guid BoardId, Guid CardId);
public sealed record UpdateCardRequest(Guid BoardId, Guid CardId, string Title, string Description);
