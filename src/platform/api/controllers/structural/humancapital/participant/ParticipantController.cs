using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Humancapital.Participant;

[Authorize]
[ApiController]
[Route("api/structural/humancapital/participant")]
[ApiExplorerSettings(GroupName = "structural.humancapital.participant")]
public sealed class ParticipantController : ControllerBase
{
    private static readonly DomainRoute ParticipantRoute = new("structural", "humancapital", "participant");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ParticipantController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterParticipantRequestModel> request, CancellationToken ct)
    {
        return Dispatch(new RegisterParticipantCommand(request.Data.ParticipantId), "participant_registered", "structural.humancapital.participant.register_failed", ct);
    }

    [HttpPost("place")]
    public Task<IActionResult> Place([FromBody] ApiRequest<PlaceParticipantRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new PlaceParticipantCommand(p.ParticipantId, p.HomeClusterId, _clock.UtcNow), "participant_placed", "structural.humancapital.participant.place_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_humancapital_participant.participant_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.humancapital.participant.not_found", $"Participant {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ParticipantReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ParticipantReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ParticipantRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterParticipantRequestModel(Guid ParticipantId);
public sealed record PlaceParticipantRequestModel(Guid ParticipantId, Guid HomeClusterId);
