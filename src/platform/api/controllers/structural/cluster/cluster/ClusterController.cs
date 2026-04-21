using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Cluster;

[Authorize]
[ApiController]
[Route("api/structural/cluster/cluster")]
[ApiExplorerSettings(GroupName = "structural.cluster.cluster")]
public sealed class ClusterController : ControllerBase
{
    private static readonly DomainRoute ClusterRoute = new("structural", "cluster", "cluster");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ClusterController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineClusterRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:cluster:{p.ClusterName}:{p.ClusterType}");
        return Dispatch(new DefineClusterCommand(id, p.ClusterName, p.ClusterType), "cluster_defined", "structural.cluster.cluster.define_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateClusterRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateClusterCommand(request.Data.ClusterId), "cluster_activated", "structural.cluster.cluster.activate_failed", ct);

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ArchiveClusterRequestModel> request, CancellationToken ct)
        => Dispatch(new ArchiveClusterCommand(request.Data.ClusterId), "cluster_archived", "structural.cluster.cluster.archive_failed", ct);

    [HttpPost("bind-authority")]
    public Task<IActionResult> BindAuthority([FromBody] ApiRequest<BindAuthorityToClusterRequestModel> request, CancellationToken ct)
        => Dispatch(new BindAuthorityToClusterCommand(request.Data.ClusterId, request.Data.AuthorityId), "cluster_authority_bound", "structural.cluster.cluster.bind_authority_failed", ct);

    [HttpPost("release-authority")]
    public Task<IActionResult> ReleaseAuthority([FromBody] ApiRequest<ReleaseAuthorityFromClusterRequestModel> request, CancellationToken ct)
        => Dispatch(new ReleaseAuthorityFromClusterCommand(request.Data.ClusterId, request.Data.AuthorityId), "cluster_authority_released", "structural.cluster.cluster.release_authority_failed", ct);

    [HttpPost("bind-administration")]
    public Task<IActionResult> BindAdministration([FromBody] ApiRequest<BindAdministrationToClusterRequestModel> request, CancellationToken ct)
        => Dispatch(new BindAdministrationToClusterCommand(request.Data.ClusterId, request.Data.AdministrationId), "cluster_administration_bound", "structural.cluster.cluster.bind_administration_failed", ct);

    [HttpPost("release-administration")]
    public Task<IActionResult> ReleaseAdministration([FromBody] ApiRequest<ReleaseAdministrationFromClusterRequestModel> request, CancellationToken ct)
        => Dispatch(new ReleaseAdministrationFromClusterCommand(request.Data.ClusterId, request.Data.AdministrationId), "cluster_administration_released", "structural.cluster.cluster.release_administration_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_cluster.cluster_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.cluster.not_found", $"Cluster {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ClusterReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ClusterReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ClusterRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DefineClusterRequestModel(string ClusterName, string ClusterType);
public sealed record ActivateClusterRequestModel(Guid ClusterId);
public sealed record ArchiveClusterRequestModel(Guid ClusterId);
public sealed record BindAuthorityToClusterRequestModel(Guid ClusterId, Guid AuthorityId);
public sealed record ReleaseAuthorityFromClusterRequestModel(Guid ClusterId, Guid AuthorityId);
public sealed record BindAdministrationToClusterRequestModel(Guid ClusterId, Guid AdministrationId);
public sealed record ReleaseAdministrationFromClusterRequestModel(Guid ClusterId, Guid AdministrationId);
