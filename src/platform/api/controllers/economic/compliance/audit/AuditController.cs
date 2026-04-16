using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Compliance.Audit;

[Authorize]
[ApiController]
[Route("api/compliance")]
[ApiExplorerSettings(GroupName = "economic.compliance.audit")]
public sealed class AuditController : ControllerBase
{
    private static readonly DomainRoute AuditRoute = new("economic", "compliance", "audit");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public AuditController(
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

    // ── audit ───────────────────────────────────────────────────

    [HttpPost("audit/create")]
    public Task<IActionResult> CreateAudit([FromBody] ApiRequest<CreateAuditRecordRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var auditRecordId = _idGenerator.Generate(
            $"economic:compliance:audit:{p.SourceDomain}:{p.SourceAggregateId}:{p.SourceEventId}");
        var cmd = new CreateAuditRecordCommand(
            auditRecordId,
            p.SourceDomain,
            p.SourceAggregateId,
            p.SourceEventId,
            p.AuditType,
            p.EvidenceSummary,
            _clock.UtcNow);
        return Dispatch(cmd, AuditRoute, "audit_record_created", "economic.compliance.audit.create_failed", ct);
    }

    [HttpPost("audit/finalize")]
    public Task<IActionResult> FinalizeAudit([FromBody] ApiRequest<FinalizeAuditRecordRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FinalizeAuditRecordCommand(p.AuditRecordId, _clock.UtcNow);
        return Dispatch(cmd, AuditRoute, "audit_record_finalized", "economic.compliance.audit.finalize_failed", ct);
    }

    [HttpGet("audit/{id:guid}")]
    public Task<IActionResult> GetAudit(Guid id, CancellationToken ct) =>
        LoadReadModel<AuditRecordReadModel>(
            id,
            "projection_economic_compliance_audit",
            "audit_record_read_model",
            "economic.compliance.audit.not_found",
            ct);

    // ── helpers ─────────────────────────────────────────────────

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

// ── Request models ──────────────────────────────────────────────

public sealed record CreateAuditRecordRequestModel(
    string SourceDomain,
    Guid SourceAggregateId,
    Guid SourceEventId,
    string AuditType,
    string EvidenceSummary);

public sealed record FinalizeAuditRecordRequestModel(Guid AuditRecordId);
