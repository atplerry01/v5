using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.DeliveryGovernance.Observability;

[Authorize]
[ApiController]
[Route("api/content/streaming/delivery-governance/observability")]
[ApiExplorerSettings(GroupName = "content.streaming.delivery_governance.observability")]
public sealed class ObservabilityController : ControllerBase
{
    private static readonly DomainRoute ObservabilityRoute = new("content", "streaming", "observability");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ObservabilityController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("capture")]
    public Task<IActionResult> Capture([FromBody] ApiRequest<CaptureObservabilityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CaptureObservabilityCommand(
                p.ObservabilityId,
                p.StreamId,
                p.ArchiveId,
                p.WindowStart,
                p.WindowEnd,
                p.Viewers,
                p.Playbacks,
                p.Errors,
                p.Drops,
                p.AverageBitrateBps,
                p.AverageLatencyMs,
                _clock.UtcNow),
            "observability_captured",
            "content.streaming.delivery_governance.observability.capture_failed",
            ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateObservabilityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new UpdateObservabilityCommand(
                p.ObservabilityId,
                p.Viewers,
                p.Playbacks,
                p.Errors,
                p.Drops,
                p.AverageBitrateBps,
                p.AverageLatencyMs,
                _clock.UtcNow),
            "observability_updated",
            "content.streaming.delivery_governance.observability.update_failed",
            ct);
    }

    [HttpPost("finalize")]
    public Task<IActionResult> Finalize([FromBody] ApiRequest<FinalizeObservabilityRequestModel> request, CancellationToken ct)
        => Dispatch(new FinalizeObservabilityCommand(request.Data.ObservabilityId, _clock.UtcNow),
            "observability_finalized", "content.streaming.delivery_governance.observability.finalize_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveObservabilityRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveObservabilityCommand(request.Data.ObservabilityId, _clock.UtcNow),
            "observability_archived", "content.streaming.delivery_governance.observability.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetObservability(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_delivery_governance_observability.observability_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.delivery_governance.observability.not_found", $"Observability {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ObservabilityReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ObservabilityReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ObservabilityRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CaptureObservabilityRequestModel(
    Guid ObservabilityId,
    Guid StreamId,
    Guid? ArchiveId,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    long Viewers,
    long Playbacks,
    long Errors,
    long Drops,
    long AverageBitrateBps,
    long AverageLatencyMs);

public sealed record UpdateObservabilityRequestModel(
    Guid ObservabilityId,
    long Viewers,
    long Playbacks,
    long Errors,
    long Drops,
    long AverageBitrateBps,
    long AverageLatencyMs);

public sealed record FinalizeObservabilityRequestModel(Guid ObservabilityId);
public sealed record ArchiveObservabilityRequestModel(Guid ObservabilityId);
