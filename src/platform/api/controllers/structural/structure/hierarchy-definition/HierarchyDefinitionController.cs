using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Structure.HierarchyDefinition;

[Authorize]
[ApiController]
[Route("api/structural/structure/hierarchy-definition")]
[ApiExplorerSettings(GroupName = "structural.structure.hierarchy_definition")]
public sealed class HierarchyDefinitionController : ControllerBase
{
    private static readonly DomainRoute HierarchyDefinitionRoute = new("structural", "structure", "hierarchy-definition");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public HierarchyDefinitionController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineHierarchyDefinitionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:structure:hierarchy-definition:{p.HierarchyName}:{p.ParentReference}");
        return Dispatch(new DefineHierarchyDefinitionCommand(id, p.HierarchyName, p.ParentReference), "hierarchy_definition_defined", "structural.structure.hierarchy_definition.define_failed", ct);
    }

    [HttpPost("validate")]
    public Task<IActionResult> Validate([FromBody] ApiRequest<ValidateHierarchyDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new ValidateHierarchyDefinitionCommand(request.Data.HierarchyDefinitionId), "hierarchy_definition_validated", "structural.structure.hierarchy_definition.validate_failed", ct);

    [HttpPost("lock")]
    public Task<IActionResult> Lock([FromBody] ApiRequest<LockHierarchyDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new LockHierarchyDefinitionCommand(request.Data.HierarchyDefinitionId), "hierarchy_definition_locked", "structural.structure.hierarchy_definition.lock_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_structure_hierarchy_definition.hierarchy_definition_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.structure.hierarchy_definition.not_found", $"HierarchyDefinition {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<HierarchyDefinitionReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize HierarchyDefinitionReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, HierarchyDefinitionRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DefineHierarchyDefinitionRequestModel(string HierarchyName, Guid ParentReference);
public sealed record ValidateHierarchyDefinitionRequestModel(Guid HierarchyDefinitionId);
public sealed record LockHierarchyDefinitionRequestModel(Guid HierarchyDefinitionId);
