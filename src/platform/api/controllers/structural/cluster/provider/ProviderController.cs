using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Provider;

[Authorize]
[ApiController]
[Route("api/structural/cluster/provider")]
[ApiExplorerSettings(GroupName = "structural.cluster.provider")]
public sealed class ProviderController : ControllerBase
{
    private static readonly DomainRoute ProviderRoute = new("structural", "cluster", "provider");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public ProviderController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterProviderRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:provider:{p.ClusterReference}:{p.ProviderName}");
        return Dispatch(new RegisterProviderCommand(id, p.ClusterReference, p.ProviderName), "provider_registered", "structural.cluster.provider.register_failed", ct);
    }

    [HttpPost("register-with-parent")]
    public Task<IActionResult> RegisterWithParent([FromBody] ApiRequest<RegisterProviderWithParentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:provider:{p.ClusterReference}:{p.ProviderName}:{p.EffectiveAt.UtcTicks}");
        return Dispatch(new RegisterProviderWithParentCommand(id, p.ClusterReference, p.ProviderName, p.EffectiveAt), "provider_registered_with_parent", "structural.cluster.provider.register_with_parent_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateProviderRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateProviderCommand(request.Data.ProviderId), "provider_activated", "structural.cluster.provider.activate_failed", ct);

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendProviderRequestModel> request, CancellationToken ct)
        => Dispatch(new SuspendProviderCommand(request.Data.ProviderId), "provider_suspended", "structural.cluster.provider.suspend_failed", ct);

    [HttpPost("reactivate")]
    public Task<IActionResult> Reactivate([FromBody] ApiRequest<ReactivateProviderRequestModel> request, CancellationToken ct)
        => Dispatch(new ReactivateProviderCommand(request.Data.ProviderId), "provider_reactivated", "structural.cluster.provider.reactivate_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireProviderRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireProviderCommand(request.Data.ProviderId), "provider_retired", "structural.cluster.provider.retire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_provider.provider_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.provider.not_found", $"Provider {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<ProviderReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize ProviderReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, ProviderRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterProviderRequestModel(Guid ClusterReference, string ProviderName);
public sealed record RegisterProviderWithParentRequestModel(Guid ClusterReference, string ProviderName, DateTimeOffset EffectiveAt);
public sealed record ActivateProviderRequestModel(Guid ProviderId);
public sealed record SuspendProviderRequestModel(Guid ProviderId);
public sealed record ReactivateProviderRequestModel(Guid ProviderId);
public sealed record RetireProviderRequestModel(Guid ProviderId);
