using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Media.LifecycleChange.Version;

[Authorize]
[ApiController]
[Route("api/content/media/lifecycle-change/version")]
[ApiExplorerSettings(GroupName = "content.media.lifecycle_change.version")]
public sealed class MediaVersionController : ControllerBase
{
    private static readonly DomainRoute VersionRoute = new("content", "media", "version");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public MediaVersionController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateMediaVersionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var versionId = _idGenerator.Generate($"content:media:lifecycle-change:version:{p.AssetRef}:{p.VersionMajor}.{p.VersionMinor}");
        return Dispatch(new CreateMediaVersionCommand(versionId, p.AssetRef, p.VersionMajor, p.VersionMinor, p.FileRef, p.PreviousVersionId, _clock.UtcNow),
            "media_version_created", "content.media.lifecycle_change.version.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateMediaVersionRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateMediaVersionCommand(request.Data.VersionId, _clock.UtcNow), "media_version_activated", "content.media.lifecycle_change.version.activate_failed", ct);

    [HttpPost("supersede")]
    public Task<IActionResult> Supersede([FromBody] ApiRequest<SupersedeMediaVersionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new SupersedeMediaVersionCommand(p.VersionId, p.SuccessorVersionId, _clock.UtcNow), "media_version_superseded", "content.media.lifecycle_change.version.supersede_failed", ct);
    }

    [HttpPost("withdraw")]
    public Task<IActionResult> Withdraw([FromBody] ApiRequest<WithdrawMediaVersionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new WithdrawMediaVersionCommand(p.VersionId, p.Reason, _clock.UtcNow), "media_version_withdrawn", "content.media.lifecycle_change.version.withdraw_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_media_lifecycle_change_version.media_version_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.media.lifecycle_change.version.not_found", $"MediaVersion {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<MediaVersionReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize MediaVersionReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, VersionRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateMediaVersionRequestModel(Guid AssetRef, int VersionMajor, int VersionMinor, Guid FileRef, Guid? PreviousVersionId);
public sealed record ActivateMediaVersionRequestModel(Guid VersionId);
public sealed record SupersedeMediaVersionRequestModel(Guid VersionId, Guid SuccessorVersionId);
public sealed record WithdrawMediaVersionRequestModel(Guid VersionId, string Reason);
