using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.StreamCore.Manifest;

[Authorize]
[ApiController]
[Route("api/content/streaming/stream-core/manifest")]
[ApiExplorerSettings(GroupName = "content.streaming.stream_core.manifest")]
public sealed class ManifestController : ControllerBase
{
    private static readonly DomainRoute ManifestRoute = new("content", "streaming", "manifest");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ManifestController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateManifestRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CreateManifestCommand(p.ManifestId, p.SourceId, p.SourceKind, _clock.UtcNow), "manifest_created", "content.streaming.stream_core.manifest.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateManifestRequestModel> request, CancellationToken ct)
        => Dispatch(new UpdateManifestCommand(request.Data.ManifestId, _clock.UtcNow), "manifest_updated", "content.streaming.stream_core.manifest.update_failed", ct);

    [HttpPost("publish")]
    public Task<IActionResult> Publish([FromBody] ApiRequest<PublishManifestRequestModel> request, CancellationToken ct)
        => Dispatch(new PublishManifestCommand(request.Data.ManifestId, _clock.UtcNow), "manifest_published", "content.streaming.stream_core.manifest.publish_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireManifestRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireManifestCommand(request.Data.ManifestId, request.Data.Reason, _clock.UtcNow), "manifest_retired", "content.streaming.stream_core.manifest.retire_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveManifestRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveManifestCommand(request.Data.ManifestId, _clock.UtcNow), "manifest_archived", "content.streaming.stream_core.manifest.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetManifest(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_stream_core_manifest.manifest_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.stream_core.manifest.not_found", $"Manifest {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ManifestReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ManifestReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ManifestRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateManifestRequestModel(Guid ManifestId, Guid SourceId, string SourceKind);
public sealed record UpdateManifestRequestModel(Guid ManifestId);
public sealed record PublishManifestRequestModel(Guid ManifestId);
public sealed record RetireManifestRequestModel(Guid ManifestId, string Reason);
public sealed record ArchiveManifestRequestModel(Guid ManifestId);
