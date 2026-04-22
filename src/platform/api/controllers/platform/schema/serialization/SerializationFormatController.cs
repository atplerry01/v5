using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Schema.Serialization;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Schema.Serialization;

[Authorize]
[ApiController]
[Route("api/platform/schema/serialization")]
[ApiExplorerSettings(GroupName = "platform.schema.serialization")]
public sealed class SerializationFormatController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "schema", "serialization");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public SerializationFormatController(
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
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterSerializationFormatRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:schema:serialization:{p.FormatName}:{p.Encoding}:{p.FormatVersion}");
        var options = p.Options.Select(o => new SerializationOptionDto(o.Key, o.Value)).ToList();
        var cmd = new RegisterSerializationFormatCommand(id, p.FormatName, p.Encoding, p.SchemaRef, options, p.RoundTripGuarantee, p.FormatVersion, _clock.UtcNow);
        return Dispatch(cmd, "serialization_format_registered", "platform.schema.serialization.register_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateSerializationFormatRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateSerializationFormatCommand(request.Data.SerializationFormatId, _clock.UtcNow);
        return Dispatch(cmd, "serialization_format_deprecated", "platform.schema.serialization.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_schema_serialization.serialization_format_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.schema.serialization.not_found", $"SerializationFormat {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<SerializationFormatReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize SerializationFormatReadModel for {id}.");
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

public sealed record RegisterSerializationFormatRequestModel(
    string FormatName,
    string Encoding,
    Guid? SchemaRef,
    IReadOnlyList<SerializationOptionRequestItem> Options,
    string RoundTripGuarantee,
    int FormatVersion);

public sealed record SerializationOptionRequestItem(string Key, string Value);

public sealed record DeprecateSerializationFormatRequestModel(Guid SerializationFormatId);
