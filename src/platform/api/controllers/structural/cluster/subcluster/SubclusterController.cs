using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Subcluster;

[Authorize]
[ApiController]
[Route("api/structural/cluster/subcluster")]
[ApiExplorerSettings(GroupName = "structural.cluster.subcluster")]
public sealed class SubclusterController : ControllerBase
{
    private static readonly DomainRoute SubclusterRoute = new("structural", "cluster", "subcluster");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public SubclusterController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineSubclusterRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:subcluster:{p.ParentClusterReference}:{p.SubclusterName}");
        return Dispatch(new DefineSubclusterCommand(id, p.ParentClusterReference, p.SubclusterName), "subcluster_defined", "structural.cluster.subcluster.define_failed", ct);
    }

    [HttpPost("define-with-parent")]
    public Task<IActionResult> DefineWithParent([FromBody] ApiRequest<DefineSubclusterWithParentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:subcluster:{p.ParentClusterReference}:{p.SubclusterName}:{p.EffectiveAt.UtcTicks}");
        return Dispatch(new DefineSubclusterWithParentCommand(id, p.ParentClusterReference, p.SubclusterName, p.EffectiveAt), "subcluster_defined_with_parent", "structural.cluster.subcluster.define_with_parent_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateSubclusterRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateSubclusterCommand(request.Data.SubclusterId), "subcluster_activated", "structural.cluster.subcluster.activate_failed", ct);

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendSubclusterRequestModel> request, CancellationToken ct)
        => Dispatch(new SuspendSubclusterCommand(request.Data.SubclusterId), "subcluster_suspended", "structural.cluster.subcluster.suspend_failed", ct);

    [HttpPost("reactivate")]
    public Task<IActionResult> Reactivate([FromBody] ApiRequest<ReactivateSubclusterRequestModel> request, CancellationToken ct)
        => Dispatch(new ReactivateSubclusterCommand(request.Data.SubclusterId), "subcluster_reactivated", "structural.cluster.subcluster.reactivate_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveSubclusterRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveSubclusterCommand(request.Data.SubclusterId), "subcluster_archived", "structural.cluster.subcluster.archive_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireSubclusterRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireSubclusterCommand(request.Data.SubclusterId), "subcluster_retired", "structural.cluster.subcluster.retire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_subcluster.subcluster_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.subcluster.not_found", $"Subcluster {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<SubclusterReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize SubclusterReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, SubclusterRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DefineSubclusterRequestModel(Guid ParentClusterReference, string SubclusterName);
public sealed record DefineSubclusterWithParentRequestModel(Guid ParentClusterReference, string SubclusterName, DateTimeOffset EffectiveAt);
public sealed record ActivateSubclusterRequestModel(Guid SubclusterId);
public sealed record SuspendSubclusterRequestModel(Guid SubclusterId);
public sealed record ReactivateSubclusterRequestModel(Guid SubclusterId);
public sealed record ArchiveSubclusterRequestModel(Guid SubclusterId);
public sealed record RetireSubclusterRequestModel(Guid SubclusterId);
