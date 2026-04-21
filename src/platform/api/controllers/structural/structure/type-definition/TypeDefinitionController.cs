using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Structure.TypeDefinition;

[Authorize]
[ApiController]
[Route("api/structural/structure/type-definition")]
[ApiExplorerSettings(GroupName = "structural.structure.type_definition")]
public sealed class TypeDefinitionController : ControllerBase
{
    private static readonly DomainRoute TypeDefinitionRoute = new("structural", "structure", "type-definition");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public TypeDefinitionController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineTypeDefinitionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:structure:type-definition:{p.TypeName}:{p.TypeCategory}");
        return Dispatch(new DefineTypeDefinitionCommand(id, p.TypeName, p.TypeCategory), "type_definition_defined", "structural.structure.type_definition.define_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateTypeDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateTypeDefinitionCommand(request.Data.TypeDefinitionId), "type_definition_activated", "structural.structure.type_definition.activate_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireTypeDefinitionRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireTypeDefinitionCommand(request.Data.TypeDefinitionId), "type_definition_retired", "structural.structure.type_definition.retire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_structure_type_definition.type_definition_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.structure.type_definition.not_found", $"TypeDefinition {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<TypeDefinitionReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize TypeDefinitionReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, TypeDefinitionRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DefineTypeDefinitionRequestModel(string TypeName, string TypeCategory);
public sealed record ActivateTypeDefinitionRequestModel(Guid TypeDefinitionId);
public sealed record RetireTypeDefinitionRequestModel(Guid TypeDefinitionId);
