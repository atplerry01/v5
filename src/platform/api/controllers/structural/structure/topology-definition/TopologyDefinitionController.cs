using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Structure.TopologyDefinition;

[Authorize]
[ApiController]
[Route("api/structural/structure/topology-definition")]
[ApiExplorerSettings(GroupName = "structural.structure.topology_definition")]
public sealed class TopologyDefinitionController : ControllerBase
{
    private static readonly DomainRoute TopologyDefinitionRoute = new("structural", "structure", "topology-definition");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public TopologyDefinitionController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateTopologyDefinitionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:structure:topology-definition:{p.DefinitionName}:{p.DefinitionKind}");
        return Dispatch(new CreateTopologyDefinitionCommand(id, p.DefinitionName, p.DefinitionKind), "topology_definition_created", "structural.structure.topology_definition.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateTopologyDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateTopologyDefinitionCommand(request.Data.TopologyDefinitionId), "topology_definition_activated", "structural.structure.topology_definition.activate_failed", ct);

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendTopologyDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new SuspendTopologyDefinitionCommand(request.Data.TopologyDefinitionId), "topology_definition_suspended", "structural.structure.topology_definition.suspend_failed", ct);

    [HttpPost("reactivate")]
    public Task<IActionResult> Reactivate([FromBody] ApiRequest<ReactivateTopologyDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new ReactivateTopologyDefinitionCommand(request.Data.TopologyDefinitionId), "topology_definition_reactivated", "structural.structure.topology_definition.reactivate_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireTopologyDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireTopologyDefinitionCommand(request.Data.TopologyDefinitionId), "topology_definition_retired", "structural.structure.topology_definition.retire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_structure_topology_definition.topology_definition_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.structure.topology_definition.not_found", $"TopologyDefinition {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<TopologyDefinitionReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize TopologyDefinitionReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, TopologyDefinitionRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record CreateTopologyDefinitionRequestModel(string DefinitionName, string DefinitionKind);
public sealed record ActivateTopologyDefinitionRequestModel(Guid TopologyDefinitionId);
public sealed record SuspendTopologyDefinitionRequestModel(Guid TopologyDefinitionId);
public sealed record ReactivateTopologyDefinitionRequestModel(Guid TopologyDefinitionId);
public sealed record RetireTopologyDefinitionRequestModel(Guid TopologyDefinitionId);
