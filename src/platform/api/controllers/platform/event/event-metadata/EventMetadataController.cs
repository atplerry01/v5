using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Event.EventMetadata;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Event.EventMetadata;

[Authorize]
[ApiController]
[Route("api/platform/event/event-metadata")]
[ApiExplorerSettings(GroupName = "platform.event.event_metadata")]
public sealed class EventMetadataController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "event", "event-metadata");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public EventMetadataController(
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

    [HttpPost("attach")]
    public Task<IActionResult> Attach([FromBody] ApiRequest<AttachEventMetadataRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:event:event-metadata:{p.EnvelopeRef}:{p.TraceId}");
        var cmd = new AttachEventMetadataCommand(id, p.EnvelopeRef, p.ExecutionHash, p.PolicyDecisionHash,
            p.ActorId, p.TraceId, p.SpanId, _clock.UtcNow);
        return Dispatch(cmd, "event_metadata_attached", "platform.event.event_metadata.attach_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_event_event_metadata.event_metadata_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.event.event_metadata.not_found", $"EventMetadata {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<EventMetadataReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize EventMetadataReadModel for {id}.");
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

public sealed record AttachEventMetadataRequestModel(
    Guid EnvelopeRef,
    string ExecutionHash,
    string PolicyDecisionHash,
    string ActorId,
    string TraceId,
    string SpanId);
