using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Spv;

[Authorize]
[ApiController]
[Route("api/structural/cluster/spv")]
[ApiExplorerSettings(GroupName = "structural.cluster.spv")]
public sealed class SpvController : ControllerBase
{
    private static readonly DomainRoute SpvRoute = new("structural", "cluster", "spv");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public SpvController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateSpvRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:spv:{p.ClusterReference}:{p.SpvName}:{p.SpvType}");
        return Dispatch(new CreateSpvCommand(id, p.ClusterReference, p.SpvName, p.SpvType), "spv_created", "structural.cluster.spv.create_failed", ct);
    }

    [HttpPost("create-with-parent")]
    public Task<IActionResult> CreateWithParent([FromBody] ApiRequest<CreateSpvWithParentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:spv:{p.ClusterReference}:{p.SpvName}:{p.SpvType}:{p.EffectiveAt.UtcTicks}");
        return Dispatch(new CreateSpvWithParentCommand(id, p.ClusterReference, p.SpvName, p.SpvType, p.EffectiveAt), "spv_created_with_parent", "structural.cluster.spv.create_with_parent_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateSpvRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateSpvCommand(request.Data.SpvId), "spv_activated", "structural.cluster.spv.activate_failed", ct);

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendSpvRequestModel> request, CancellationToken ct)
        => Dispatch(new SuspendSpvCommand(request.Data.SpvId), "spv_suspended", "structural.cluster.spv.suspend_failed", ct);

    [HttpPost("close")]
    public Task<IActionResult> Close([FromBody] ApiRequest<CloseSpvRequestModel> request, CancellationToken ct)
        => Dispatch(new CloseSpvCommand(request.Data.SpvId), "spv_closed", "structural.cluster.spv.close_failed", ct);

    [HttpPost("reactivate")]
    public Task<IActionResult> Reactivate([FromBody] ApiRequest<ReactivateSpvRequestModel> request, CancellationToken ct)
        => Dispatch(new ReactivateSpvCommand(request.Data.SpvId), "spv_reactivated", "structural.cluster.spv.reactivate_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireSpvRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireSpvCommand(request.Data.SpvId), "spv_retired", "structural.cluster.spv.retire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_spv.spv_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.spv.not_found", $"Spv {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<SpvReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize SpvReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, SpvRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateSpvRequestModel(Guid ClusterReference, string SpvName, string SpvType);
public sealed record CreateSpvWithParentRequestModel(Guid ClusterReference, string SpvName, string SpvType, DateTimeOffset EffectiveAt);
public sealed record ActivateSpvRequestModel(Guid SpvId);
public sealed record SuspendSpvRequestModel(Guid SpvId);
public sealed record CloseSpvRequestModel(Guid SpvId);
public sealed record ReactivateSpvRequestModel(Guid SpvId);
public sealed record RetireSpvRequestModel(Guid SpvId);
