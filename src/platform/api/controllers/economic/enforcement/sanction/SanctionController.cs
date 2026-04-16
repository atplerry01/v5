using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Enforcement.Sanction;

[Authorize]
[ApiController]
[Route("api/enforcement")]
[ApiExplorerSettings(GroupName = "economic.enforcement.sanction")]
public sealed class SanctionController : ControllerBase
{
    private static readonly DomainRoute SanctionRoute = new("economic", "enforcement", "sanction");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public SanctionController(
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

    [HttpPost("sanction/issue")]
    public Task<IActionResult> IssueSanction([FromBody] ApiRequest<IssueSanctionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var sanctionId = _idGenerator.Generate(
            $"economic:enforcement:sanction:{p.SubjectId}:{p.Type}:{p.Scope}:{now.UtcTicks}");
        var cmd = new IssueSanctionCommand(
            sanctionId,
            p.SubjectId,
            p.Type,
            p.Scope,
            p.Reason,
            p.EffectiveAt ?? now,
            p.ExpiresAt,
            now);
        return Dispatch(cmd, SanctionRoute, "sanction_issued", "economic.enforcement.sanction.issue_failed", ct);
    }

    [HttpPost("sanction/activate")]
    public Task<IActionResult> ActivateSanction([FromBody] ApiRequest<SanctionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateSanctionCommand(request.Data.SanctionId, _clock.UtcNow);
        return Dispatch(cmd, SanctionRoute, "sanction_activated", "economic.enforcement.sanction.activate_failed", ct);
    }

    [HttpPost("sanction/revoke")]
    public Task<IActionResult> RevokeSanction([FromBody] ApiRequest<RevokeSanctionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RevokeSanctionCommand(p.SanctionId, p.RevocationReason, _clock.UtcNow);
        return Dispatch(cmd, SanctionRoute, "sanction_revoked", "economic.enforcement.sanction.revoke_failed", ct);
    }

    [HttpGet("sanction/{id:guid}")]
    public Task<IActionResult> GetSanction(Guid id, CancellationToken ct) =>
        LoadReadModel<SanctionReadModel>(
            id,
            "projection_economic_enforcement_sanction",
            "sanction_read_model",
            "economic.enforcement.sanction.not_found",
            ct);

    [HttpGet("sanction/subject/{subjectId:guid}")]
    public async Task<IActionResult> GetSanctionsBySubject(Guid subjectId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_economic_enforcement_sanction.sanction_read_model " +
            "WHERE state->>'subjectId' = @subjectId " +
            "ORDER BY (state->>'issuedAt') DESC", conn);
        cmd.Parameters.AddWithValue("subjectId", subjectId.ToString());

        var results = new List<SanctionReadModel>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var json = reader.GetString(0);
            var model = JsonSerializer.Deserialize<SanctionReadModel>(json)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize SanctionReadModel for subject {subjectId}.");
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

public sealed record IssueSanctionRequestModel(
    Guid SubjectId,
    string Type,
    string Scope,
    string Reason,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt);

public sealed record SanctionIdRequestModel(Guid SanctionId);

public sealed record RevokeSanctionRequestModel(
    Guid SanctionId,
    string RevocationReason);
