using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Structure.Classification;

[Authorize]
[ApiController]
[Route("api/structural/structure/classification")]
[ApiExplorerSettings(GroupName = "structural.structure.classification")]
public sealed class ClassificationController : ControllerBase
{
    private static readonly DomainRoute ClassificationRoute = new("structural", "structure", "classification");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ClassificationController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineClassificationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:structure:classification:{p.ClassificationName}:{p.ClassificationCategory}");
        return Dispatch(new DefineClassificationCommand(id, p.ClassificationName, p.ClassificationCategory), "classification_defined", "structural.structure.classification.define_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateClassificationRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateClassificationCommand(request.Data.ClassificationId), "classification_activated", "structural.structure.classification.activate_failed", ct);

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DeprecateClassificationRequestModel> request, CancellationToken ct)
        => Dispatch(new DeprecateClassificationCommand(request.Data.ClassificationId), "classification_deprecated", "structural.structure.classification.deprecate_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_structure_classification.classification_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.structure.classification.not_found", $"Classification {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ClassificationReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ClassificationReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ClassificationRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DefineClassificationRequestModel(string ClassificationName, string ClassificationCategory);
public sealed record ActivateClassificationRequestModel(Guid ClassificationId);
public sealed record DeprecateClassificationRequestModel(Guid ClassificationId);
