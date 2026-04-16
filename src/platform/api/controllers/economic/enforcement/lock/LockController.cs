using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Enforcement.Lock;

[Authorize]
[ApiController]
[Route("api/enforcement")]
[ApiExplorerSettings(GroupName = "economic.enforcement.lock")]
public sealed class LockController : ControllerBase
{
    private static readonly DomainRoute LockRoute = new("economic", "enforcement", "lock");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public LockController(
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

    [HttpPost("lock/lock")]
    public Task<IActionResult> LockSystem([FromBody] ApiRequest<LockSystemRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var lockId = _idGenerator.Generate(
            $"economic:enforcement:lock:{p.SubjectId}:{p.Scope}:{now.UtcTicks}");
        var cmd = new LockSystemCommand(
            lockId,
            p.SubjectId,
            p.Scope,
            p.Reason,
            now);
        return Dispatch(cmd, LockRoute, "system_locked", "economic.enforcement.lock.lock_failed", ct);
    }

    [HttpPost("lock/unlock")]
    public Task<IActionResult> UnlockSystem([FromBody] ApiRequest<UnlockSystemRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UnlockSystemCommand(p.LockId, p.UnlockReason, _clock.UtcNow);
        return Dispatch(cmd, LockRoute, "system_unlocked", "economic.enforcement.lock.unlock_failed", ct);
    }

    [HttpGet("lock/{id:guid}")]
    public Task<IActionResult> GetLock(Guid id, CancellationToken ct) =>
        LoadReadModel<LockReadModel>(
            id,
            "projection_economic_enforcement_lock",
            "lock_read_model",
            "economic.enforcement.lock.not_found",
            ct);

    [HttpGet("lock/subject/{subjectId:guid}")]
    public async Task<IActionResult> GetLocksBySubject(Guid subjectId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_economic_enforcement_lock.lock_read_model " +
            "WHERE state->>'subjectId' = @subjectId " +
            "ORDER BY (state->>'lockedAt') DESC", conn);
        cmd.Parameters.AddWithValue("subjectId", subjectId.ToString());

        var results = new List<LockReadModel>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var json = reader.GetString(0);
            var model = JsonSerializer.Deserialize<LockReadModel>(json)
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize LockReadModel for subject {subjectId}.");
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

public sealed record LockSystemRequestModel(
    Guid SubjectId,
    string Scope,
    string Reason);

public sealed record UnlockSystemRequestModel(
    Guid LockId,
    string UnlockReason);
