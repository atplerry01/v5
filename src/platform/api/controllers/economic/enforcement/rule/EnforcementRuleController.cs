using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Enforcement.Rule;

[Authorize]
[ApiController]
[Route("api/enforcement")]
[ApiExplorerSettings(GroupName = "economic.enforcement.rule")]
public sealed class EnforcementRuleController : ControllerBase
{
    private static readonly DomainRoute RuleRoute = new("economic", "enforcement", "rule");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public EnforcementRuleController(
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

    [HttpPost("rule/define")]
    public Task<IActionResult> DefineRule([FromBody] ApiRequest<DefineEnforcementRuleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var ruleId = _idGenerator.Generate($"economic:enforcement:rule:{p.RuleCode}");
        var cmd = new DefineEnforcementRuleCommand(
            ruleId,
            p.RuleCode,
            p.RuleName,
            p.RuleCategory,
            p.Scope,
            p.Severity,
            p.Description,
            _clock.UtcNow);
        return Dispatch(cmd, RuleRoute, "enforcement_rule_defined", "economic.enforcement.rule.define_failed", ct);
    }

    [HttpPost("rule/activate")]
    public Task<IActionResult> ActivateRule([FromBody] ApiRequest<EnforcementRuleIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateEnforcementRuleCommand(request.Data.RuleId);
        return Dispatch(cmd, RuleRoute, "enforcement_rule_activated", "economic.enforcement.rule.activate_failed", ct);
    }

    [HttpPost("rule/disable")]
    public Task<IActionResult> DisableRule([FromBody] ApiRequest<EnforcementRuleIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DisableEnforcementRuleCommand(request.Data.RuleId);
        return Dispatch(cmd, RuleRoute, "enforcement_rule_disabled", "economic.enforcement.rule.disable_failed", ct);
    }

    [HttpPost("rule/retire")]
    public Task<IActionResult> RetireRule([FromBody] ApiRequest<EnforcementRuleIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RetireEnforcementRuleCommand(request.Data.RuleId);
        return Dispatch(cmd, RuleRoute, "enforcement_rule_retired", "economic.enforcement.rule.retire_failed", ct);
    }

    [HttpGet("rule/{id:guid}")]
    public Task<IActionResult> GetRule(Guid id, CancellationToken ct) =>
        LoadReadModel<EnforcementRuleReadModel>(
            id,
            "projection_economic_enforcement_rule",
            "enforcement_rule_read_model",
            "economic.enforcement.rule.not_found",
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

public sealed record DefineEnforcementRuleRequestModel(
    string RuleCode,
    string RuleName,
    string RuleCategory,
    string Scope,
    string Severity,
    string Description);

public sealed record EnforcementRuleIdRequestModel(Guid RuleId);
