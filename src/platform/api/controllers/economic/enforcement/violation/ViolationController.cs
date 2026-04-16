using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Enforcement.Violation;

[Authorize]
[ApiController]
[Route("api/enforcement")]
[ApiExplorerSettings(GroupName = "economic.enforcement.violation")]
public sealed class ViolationController : ControllerBase
{
    private static readonly DomainRoute ViolationRoute = new("economic", "enforcement", "violation");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public ViolationController(
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

    [HttpPost("violation/detect")]
    public Task<IActionResult> DetectViolation([FromBody] ApiRequest<DetectViolationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var violationId = _idGenerator.Generate(
            $"economic:enforcement:violation:{p.RuleId}:{p.SourceReference}:{_clock.UtcNow.UtcTicks}");
        var cmd = new DetectViolationCommand(
            violationId,
            p.RuleId,
            p.SourceReference,
            p.Reason,
            p.Severity,
            p.RecommendedAction,
            _clock.UtcNow);
        return Dispatch(cmd, ViolationRoute, "violation_detected", "economic.enforcement.violation.detect_failed", ct);
    }

    [HttpPost("violation/acknowledge")]
    public Task<IActionResult> AcknowledgeViolation([FromBody] ApiRequest<ViolationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new AcknowledgeViolationCommand(request.Data.ViolationId, _clock.UtcNow);
        return Dispatch(cmd, ViolationRoute, "violation_acknowledged", "economic.enforcement.violation.acknowledge_failed", ct);
    }

    [HttpPost("violation/resolve")]
    public Task<IActionResult> ResolveViolation([FromBody] ApiRequest<ResolveViolationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ResolveViolationCommand(p.ViolationId, p.Resolution, _clock.UtcNow);
        return Dispatch(cmd, ViolationRoute, "violation_resolved", "economic.enforcement.violation.resolve_failed", ct);
    }

    [HttpGet("violation/{id:guid}")]
    public Task<IActionResult> GetViolation(Guid id, CancellationToken ct) =>
        LoadReadModel<ViolationReadModel>(
            id,
            "projection_economic_enforcement_violation",
            "violation_read_model",
            "economic.enforcement.violation.not_found",
            ct);

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

public sealed record DetectViolationRequestModel(
    Guid RuleId,
    Guid SourceReference,
    string Reason,
    string Severity,
    string RecommendedAction);

public sealed record ViolationIdRequestModel(Guid ViolationId);

public sealed record ResolveViolationRequestModel(
    Guid ViolationId,
    string Resolution);
