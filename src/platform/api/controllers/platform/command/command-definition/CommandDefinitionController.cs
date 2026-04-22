using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Command.CommandDefinition;

[Authorize]
[ApiController]
[Route("api/platform/command/command-definition")]
[ApiExplorerSettings(GroupName = "platform.command.command_definition")]
public sealed class CommandDefinitionController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "command", "command-definition");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public CommandDefinitionController(
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

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineCommandRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:command:command-definition:{p.TypeName}:{p.Version}");
        var cmd = new DefineCommandCommand(id, p.TypeName, p.Version, p.SchemaId,
            p.OwnerClassification, p.OwnerContext, p.OwnerDomain, _clock.UtcNow);
        return Dispatch(cmd, "command_defined", "platform.command.command_definition.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateCommandDefinitionRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateCommandDefinitionCommand(request.Data.CommandDefinitionId, _clock.UtcNow);
        return Dispatch(cmd, "command_deprecated", "platform.command.command_definition.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_command_command_definition.command_definition_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.command.command_definition.not_found", $"CommandDefinition {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<CommandDefinitionReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize CommandDefinitionReadModel for {id}.");
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

public sealed record DefineCommandRequestModel(
    string TypeName,
    string Version,
    string SchemaId,
    string OwnerClassification,
    string OwnerContext,
    string OwnerDomain);

public sealed record DeprecateCommandDefinitionRequestModel(Guid CommandDefinitionId);
