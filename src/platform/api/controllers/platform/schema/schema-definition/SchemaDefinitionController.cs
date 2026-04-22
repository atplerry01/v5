using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Schema.SchemaDefinition;

[Authorize]
[ApiController]
[Route("api/platform/schema/schema-definition")]
[ApiExplorerSettings(GroupName = "platform.schema.schema_definition")]
public sealed class SchemaDefinitionController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "schema", "schema-definition");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public SchemaDefinitionController(
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

    [HttpPost("draft")]
    public Task<IActionResult> Draft([FromBody] ApiRequest<DraftSchemaDefinitionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:schema:schema-definition:{p.SchemaName}:{p.Version}");
        var fields = p.Fields.Select(f => new FieldDescriptorDto(f.FieldName, f.FieldType, f.Required, f.DefaultValue, f.Description)).ToList();
        var cmd = new DraftSchemaDefinitionCommand(id, p.SchemaName, p.Version, fields, p.CompatibilityMode, _clock.UtcNow);
        return Dispatch(cmd, "schema_definition_drafted", "platform.schema.schema_definition.draft_failed", ct);
    }

    [HttpPost("publish")]
    public Task<IActionResult> Publish([FromBody] ApiRequest<PublishSchemaDefinitionRequestModel> request, CancellationToken ct)
    {
        var cmd = new PublishSchemaDefinitionCommand(request.Data.SchemaDefinitionId, _clock.UtcNow);
        return Dispatch(cmd, "schema_definition_published", "platform.schema.schema_definition.publish_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateSchemaDefinitionRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateSchemaDefinitionCommand(request.Data.SchemaDefinitionId, _clock.UtcNow);
        return Dispatch(cmd, "schema_definition_deprecated", "platform.schema.schema_definition.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_schema_schema_definition.schema_definition_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.schema.schema_definition.not_found", $"SchemaDefinition {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<SchemaDefinitionReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize SchemaDefinitionReadModel for {id}.");
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

public sealed record DraftSchemaDefinitionRequestModel(
    string SchemaName,
    int Version,
    IReadOnlyList<FieldDescriptorRequestItem> Fields,
    string CompatibilityMode);

public sealed record FieldDescriptorRequestItem(
    string FieldName,
    string FieldType,
    bool Required,
    string? DefaultValue,
    string? Description);

public sealed record PublishSchemaDefinitionRequestModel(Guid SchemaDefinitionId);
public sealed record DeprecateSchemaDefinitionRequestModel(Guid SchemaDefinitionId);
