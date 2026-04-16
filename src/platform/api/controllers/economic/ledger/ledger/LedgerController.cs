using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Ledger.Ledger;

[Authorize]
[ApiController]
[Route("api/economic/ledger")]
[ApiExplorerSettings(GroupName = "economic.ledger.ledger")]
public sealed class LedgerController : ControllerBase
{
    private static readonly DomainRoute LedgerRoute = new("economic", "ledger", "ledger");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public LedgerController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("open")]
    public async Task<IActionResult> OpenLedger(
        [FromBody] ApiRequest<OpenLedgerRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var ledgerId = _idGenerator.Generate(
            $"economic:ledger:ledger:{p.Reference}:{p.Currency}");

        var command = new OpenLedgerCommand(ledgerId, p.Currency);

        var result = await _dispatcher.DispatchAsync(command, LedgerRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new OpenLedgerResponseModel(ledgerId), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.open_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow,
                result.CorrelationId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            """
            SELECT state
            FROM projection_economic_ledger_ledger.ledger_read_model
            WHERE aggregate_id = @id
            LIMIT 1
            """, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return NotFound(ApiResponse.Fail(
                "economic.ledger.not_found",
                $"Ledger {id} not found.",
                _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<LedgerReadModel>(stateJson)
            ?? new LedgerReadModel { LedgerId = id };

        return Ok(ApiResponse.Ok(model, _clock.UtcNow));
    }
}

public sealed record OpenLedgerRequestModel(
    string Reference,
    string Currency);

public sealed record OpenLedgerResponseModel(Guid LedgerId);
