using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Schema.Versioning;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Schema.Versioning;

[Authorize]
[ApiController]
[Route("api/platform/schema/versioning")]
[ApiExplorerSettings(GroupName = "platform.schema.versioning")]
public sealed class VersioningRuleController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "schema", "versioning");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public VersioningRuleController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterVersioningRuleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:schema:versioning:{p.SchemaRef}:{p.FromVersion}:{p.ToVersion}");
        var changes = p.ChangeSummary.Select(c => new SchemaChangeDto(c.ChangeType, c.FieldName, c.Impact)).ToList();
        var cmd = new RegisterVersioningRuleCommand(id, p.SchemaRef, p.FromVersion, p.ToVersion, p.EvolutionClass, changes, _clock.UtcNow);
        return Dispatch(cmd, "versioning_rule_registered", "platform.schema.versioning.register_failed", ct);
    }

    [HttpPost("issue-verdict")]
    public Task<IActionResult> IssueVerdict([FromBody] ApiRequest<IssueVersioningVerdictRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new IssueVersioningVerdictCommand(p.VersioningRuleId, p.Verdict, _clock.UtcNow);
        return Dispatch(cmd, "versioning_verdict_issued", "platform.schema.versioning.issue_verdict_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_schema_versioning.versioning_rule_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.schema.versioning.not_found", $"VersioningRule {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<VersioningRuleReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize VersioningRuleReadModel for {id}.");
        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }

    private Guid RequestCorrelationId()
    {
        if (HttpContext is { } ctx
            && ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var values)
            && Guid.TryParse(values.ToString(), out var parsed))
            return parsed;
        return Guid.Empty;
    }
}

public sealed record RegisterVersioningRuleRequestModel(
    Guid SchemaRef,
    int FromVersion,
    int ToVersion,
    string EvolutionClass,
    IReadOnlyList<SchemaChangeRequestItem> ChangeSummary);

public sealed record SchemaChangeRequestItem(string ChangeType, string FieldName, string Impact);

public sealed record IssueVersioningVerdictRequestModel(Guid VersioningRuleId, string Verdict);
