using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Media.TechnicalProcessing.Processing;

[Authorize]
[ApiController]
[Route("api/content/media/technical-processing/processing")]
[ApiExplorerSettings(GroupName = "content.media.technical_processing.processing")]
public sealed class MediaProcessingController : ControllerBase
{
    private static readonly DomainRoute ProcessingRoute = new("content", "media", "processing");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public MediaProcessingController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("request")]
    public Task<IActionResult> RequestProcessing([FromBody] ApiRequest<RequestMediaProcessingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var jobId = _idGenerator.Generate($"content:media:technical-processing:processing:{p.Kind}:{p.InputRef}");
        return Dispatch(new RequestMediaProcessingCommand(jobId, p.Kind, p.InputRef, _clock.UtcNow), "media_processing_requested", "content.media.technical_processing.processing.request_failed", ct);
    }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartMediaProcessingRequestModel> request, CancellationToken ct)
        => Dispatch(new StartMediaProcessingCommand(request.Data.JobId, _clock.UtcNow), "media_processing_started", "content.media.technical_processing.processing.start_failed", ct);

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteMediaProcessingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CompleteMediaProcessingCommand(p.JobId, p.OutputRef, _clock.UtcNow), "media_processing_completed", "content.media.technical_processing.processing.complete_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailMediaProcessingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new FailMediaProcessingCommand(p.JobId, p.Reason, _clock.UtcNow), "media_processing_failed", "content.media.technical_processing.processing.fail_failed", ct);
    }

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<CancelMediaProcessingRequestModel> request, CancellationToken ct)
        => Dispatch(new CancelMediaProcessingCommand(request.Data.JobId, _clock.UtcNow), "media_processing_cancelled", "content.media.technical_processing.processing.cancel_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_media_technical_processing_processing.media_processing_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.media.technical_processing.processing.not_found", $"MediaProcessing {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<MediaProcessingReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize MediaProcessingReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ProcessingRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RequestMediaProcessingRequestModel(string Kind, Guid InputRef);
public sealed record StartMediaProcessingRequestModel(Guid JobId);
public sealed record CompleteMediaProcessingRequestModel(Guid JobId, Guid OutputRef);
public sealed record FailMediaProcessingRequestModel(Guid JobId, string Reason);
public sealed record CancelMediaProcessingRequestModel(Guid JobId);
