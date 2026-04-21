using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Lifecycle;

[Authorize]
[ApiController]
[Route("api/structural/cluster/lifecycle")]
[ApiExplorerSettings(GroupName = "structural.cluster.lifecycle")]
public sealed class LifecycleController : ControllerBase
{
    private static readonly DomainRoute LifecycleRoute = new("structural", "cluster", "lifecycle");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public LifecycleController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineLifecycleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:lifecycle:{p.ClusterReference}:{p.LifecycleName}");
        return Dispatch(new DefineLifecycleCommand(id, p.ClusterReference, p.LifecycleName), "lifecycle_defined", "structural.cluster.lifecycle.define_failed", ct);
    }

    [HttpPost("transition")]
    public Task<IActionResult> Transition([FromBody] ApiRequest<TransitionLifecycleRequestModel> request, CancellationToken ct)
        => Dispatch(new TransitionLifecycleCommand(request.Data.LifecycleId), "lifecycle_transitioned", "structural.cluster.lifecycle.transition_failed", ct);

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteLifecycleRequestModel> request, CancellationToken ct)
        => Dispatch(new CompleteLifecycleCommand(request.Data.LifecycleId), "lifecycle_completed", "structural.cluster.lifecycle.complete_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_lifecycle.lifecycle_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.lifecycle.not_found", $"Lifecycle {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<LifecycleReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize LifecycleReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, LifecycleRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record DefineLifecycleRequestModel(Guid ClusterReference, string LifecycleName);
public sealed record TransitionLifecycleRequestModel(Guid LifecycleId);
public sealed record CompleteLifecycleRequestModel(Guid LifecycleId);
