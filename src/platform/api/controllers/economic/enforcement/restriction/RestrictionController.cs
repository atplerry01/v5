using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Enforcement.Restriction;

[Authorize]
[ApiController]
[Route("api/enforcement")]
[ApiExplorerSettings(GroupName = "economic.enforcement.restriction")]
public sealed class RestrictionController : ControllerBase
{
    private static readonly DomainRoute RestrictionRoute = new("economic", "enforcement", "restriction");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public RestrictionController(
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

    [HttpPost("restriction/apply")]
    public Task<IActionResult> ApplyRestriction([FromBody] ApiRequest<ApplyRestrictionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var restrictionId = _idGenerator.Generate(
            $"economic:enforcement:restriction:{p.SubjectId}:{p.Scope}:{now.UtcTicks}");
        var cmd = new ApplyRestrictionCommand(
            restrictionId,
            p.SubjectId,
            p.Scope,
            p.Reason,
            now);
        return Dispatch(cmd, RestrictionRoute, "restriction_applied", "economic.enforcement.restriction.apply_failed", ct);
    }

    [HttpPost("restriction/update")]
    public Task<IActionResult> UpdateRestriction([FromBody] ApiRequest<UpdateRestrictionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateRestrictionCommand(p.RestrictionId, p.NewScope, p.NewReason, _clock.UtcNow);
        return Dispatch(cmd, RestrictionRoute, "restriction_updated", "economic.enforcement.restriction.update_failed", ct);
    }

    [HttpPost("restriction/remove")]
    public Task<IActionResult> RemoveRestriction([FromBody] ApiRequest<RemoveRestrictionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveRestrictionCommand(p.RestrictionId, p.RemovalReason, _clock.UtcNow);
        return Dispatch(cmd, RestrictionRoute, "restriction_removed", "economic.enforcement.restriction.remove_failed", ct);
    }

    [HttpGet("restriction/{id:guid}")]
    public Task<IActionResult> GetRestriction(Guid id, CancellationToken ct) =>
        LoadReadModel<RestrictionReadModel>(
            id,
            "projection_economic_enforcement_restriction",
            "restriction_read_model",
            "economic.enforcement.restriction.not_found",
            ct);

    [HttpGet("restriction/subject/{subjectId:guid}")]
    public async Task<IActionResult> GetRestrictionsBySubject(Guid subjectId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_economic_enforcement_restriction.restriction_read_model " +
            "WHERE state->>'subjectId' = @subjectId " +
            "ORDER BY (state->>'appliedAt') DESC", conn);
        cmd.Parameters.AddWithValue("subjectId", subjectId.ToString());

        var results = new List<RestrictionReadModel>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var json = reader.GetString(0);
            var model = JsonSerializer.Deserialize<RestrictionReadModel>(json)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize RestrictionReadModel for subject {subjectId}.");
            results.Add(model);
        }

        return Ok(ApiResponse.Ok(results, this.RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, DomainRoute route, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow));
    }

    private async Task<IActionResult> LoadReadModel<TReadModel>(
        Guid id,
        string schema,
        string table,
        string notFoundCode,
        CancellationToken ct)
        where TReadModel : class
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {schema}.{table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail(notFoundCode, $"{typeof(TReadModel).Name} {id} not found.", _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<TReadModel>(stateJson)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize {typeof(TReadModel).Name} for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, this.RequestCorrelationId(), _clock.UtcNow));
    }
}

public sealed record ApplyRestrictionRequestModel(
    Guid SubjectId,
    string Scope,
    string Reason);

public sealed record UpdateRestrictionRequestModel(
    Guid RestrictionId,
    string NewScope,
    string NewReason);

public sealed record RemoveRestrictionRequestModel(
    Guid RestrictionId,
    string RemovalReason);
