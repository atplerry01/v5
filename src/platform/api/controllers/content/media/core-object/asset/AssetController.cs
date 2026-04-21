using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Media.CoreObject.Asset;

[Authorize]
[ApiController]
[Route("api/content/media/core-object/asset")]
[ApiExplorerSettings(GroupName = "content.media.core_object.asset")]
public sealed class AssetController : ControllerBase
{
    private static readonly DomainRoute AssetRoute = new("content", "media", "asset");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public AssetController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var assetId = _idGenerator.Generate($"content:media:core-object:asset:{p.Title}:{p.Classification}");
        var cmd = new CreateAssetCommand(assetId, p.Title, p.Classification, _clock.UtcNow);
        return Dispatch(cmd, "asset_created", "content.media.core_object.asset.create_failed", ct);
    }

    [HttpPost("rename")]
    public Task<IActionResult> Rename([FromBody] ApiRequest<RenameAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new RenameAssetCommand(p.AssetId, p.NewTitle, _clock.UtcNow), "asset_renamed", "content.media.core_object.asset.rename_failed", ct);
    }

    [HttpPost("reclassify")]
    public Task<IActionResult> Reclassify([FromBody] ApiRequest<ReclassifyAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new ReclassifyAssetCommand(p.AssetId, p.NewClassification, _clock.UtcNow), "asset_reclassified", "content.media.core_object.asset.reclassify_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateAssetRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateAssetCommand(request.Data.AssetId, _clock.UtcNow), "asset_activated", "content.media.core_object.asset.activate_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireAssetRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireAssetCommand(request.Data.AssetId, _clock.UtcNow), "asset_retired", "content.media.core_object.asset.retire_failed", ct);

    [HttpPost("assign-kind")]
    public Task<IActionResult> AssignKind([FromBody] ApiRequest<AssignAssetKindRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new AssignAssetKindCommand(p.AssetId, p.NewKind, _clock.UtcNow), "asset_kind_assigned", "content.media.core_object.asset.assign_kind_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsset(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_media_core_object_asset.asset_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.media.core_object.asset.not_found", $"Asset {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<AssetReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize AssetReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, AssetRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateAssetRequestModel(string Title, string Classification);
public sealed record RenameAssetRequestModel(Guid AssetId, string NewTitle);
public sealed record ReclassifyAssetRequestModel(Guid AssetId, string NewClassification);
public sealed record ActivateAssetRequestModel(Guid AssetId);
public sealed record RetireAssetRequestModel(Guid AssetId);
public sealed record AssignAssetKindRequestModel(Guid AssetId, string NewKind);
