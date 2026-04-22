using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Event.EventDefinition;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Event.EventDefinition;

[Authorize]
[ApiController]
[Route("api/platform/event/event-definition")]
[ApiExplorerSettings(GroupName = "platform.event.event_definition")]
public sealed class EventDefinitionController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "event", "event-definition");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public EventDefinitionController(
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
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineEventRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:event:event-definition:{p.TypeName}:{p.Version}");
        var cmd = new DefineEventCommand(id, p.TypeName, p.Version, p.SchemaId,
            p.SourceClassification, p.SourceContext, p.SourceDomain, _clock.UtcNow);
        return Dispatch(cmd, "event_defined", "platform.event.event_definition.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateEventDefinitionRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateEventDefinitionCommand(request.Data.EventDefinitionId, _clock.UtcNow);
        return Dispatch(cmd, "event_definition_deprecated", "platform.event.event_definition.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_event_event_definition.event_definition_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.event.event_definition.not_found", $"EventDefinition {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<EventDefinitionReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize EventDefinitionReadModel for {id}.");
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

public sealed record DefineEventRequestModel(
    string TypeName,
    string Version,
    string SchemaId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain);

public sealed record DeprecateEventDefinitionRequestModel(Guid EventDefinitionId);
