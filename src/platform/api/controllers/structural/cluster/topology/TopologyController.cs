using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Topology;

[Authorize]
[ApiController]
[Route("api/structural/cluster/topology")]
[ApiExplorerSettings(GroupName = "structural.cluster.topology")]
public sealed class TopologyController : ControllerBase
{
    private static readonly DomainRoute TopologyRoute = new("structural", "cluster", "topology");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public TopologyController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineTopologyRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:topology:{p.ClusterReference}:{p.TopologyName}");
        return Dispatch(new DefineTopologyCommand(id, p.ClusterReference, p.TopologyName), "topology_defined", "structural.cluster.topology.define_failed", ct);
    }

    [HttpPost("validate")]
    public Task<IActionResult> Validate([FromBody] ApiRequest<ValidateTopologyRequestModel> request, CancellationToken ct)
        => Dispatch(new ValidateTopologyCommand(request.Data.TopologyId), "topology_validated", "structural.cluster.topology.validate_failed", ct);

    [HttpPost("lock")]
    public Task<IActionResult> Lock([FromBody] ApiRequest<LockTopologyRequestModel> request, CancellationToken ct)
        => Dispatch(new LockTopologyCommand(request.Data.TopologyId), "topology_locked", "structural.cluster.topology.lock_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_topology.topology_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.topology.not_found", $"Topology {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<TopologyReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize TopologyReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, TopologyRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DefineTopologyRequestModel(Guid ClusterReference, string TopologyName);
public sealed record ValidateTopologyRequestModel(Guid TopologyId);
public sealed record LockTopologyRequestModel(Guid TopologyId);
