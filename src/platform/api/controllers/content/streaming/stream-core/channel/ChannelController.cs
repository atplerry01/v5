using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.StreamCore.Channel;

[Authorize]
[ApiController]
[Route("api/content/streaming/stream-core/channel")]
[ApiExplorerSettings(GroupName = "content.streaming.stream_core.channel")]
public sealed class ChannelController : ControllerBase
{
    private static readonly DomainRoute ChannelRoute = new("content", "streaming", "channel");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ChannelController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateChannelRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new CreateChannelCommand(p.ChannelId, p.StreamId, p.Name, p.Mode, _clock.UtcNow), "channel_created", "content.streaming.stream_core.channel.create_failed", ct);
    }

    [HttpPost("rename")]
    public Task<IActionResult> Rename([FromBody] ApiRequest<RenameChannelRequestModel> request, CancellationToken ct)
        => Dispatch(new RenameChannelCommand(request.Data.ChannelId, request.Data.NewName, _clock.UtcNow), "channel_renamed", "content.streaming.stream_core.channel.rename_failed", ct);

    [HttpPost("enable")]
    public Task<IActionResult> Enable([FromBody] ApiRequest<EnableChannelRequestModel> request, CancellationToken ct)
        => Dispatch(new EnableChannelCommand(request.Data.ChannelId, _clock.UtcNow), "channel_enabled", "content.streaming.stream_core.channel.enable_failed", ct);

    [HttpPost("disable")]
    public Task<IActionResult> Disable([FromBody] ApiRequest<DisableChannelRequestModel> request, CancellationToken ct)
        => Dispatch(new DisableChannelCommand(request.Data.ChannelId, request.Data.Reason, _clock.UtcNow), "channel_disabled", "content.streaming.stream_core.channel.disable_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveChannelRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveChannelCommand(request.Data.ChannelId, _clock.UtcNow), "channel_archived", "content.streaming.stream_core.channel.archive_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetChannel(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_stream_core_channel.channel_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.stream_core.channel.not_found", $"Channel {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ChannelReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ChannelReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ChannelRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateChannelRequestModel(Guid ChannelId, Guid StreamId, string Name, string Mode);
public sealed record RenameChannelRequestModel(Guid ChannelId, string NewName);
public sealed record EnableChannelRequestModel(Guid ChannelId);
public sealed record DisableChannelRequestModel(Guid ChannelId, string Reason);
public sealed record ArchiveChannelRequestModel(Guid ChannelId);
