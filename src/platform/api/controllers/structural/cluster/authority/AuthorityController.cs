using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Authority;

[Authorize]
[ApiController]
[Route("api/structural/cluster/authority")]
[ApiExplorerSettings(GroupName = "structural.cluster.authority")]
public sealed class AuthorityController : ControllerBase
{
    private static readonly DomainRoute AuthorityRoute = new("structural", "cluster", "authority");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public AuthorityController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("establish")]
    public Task<IActionResult> Establish([FromBody] ApiRequest<EstablishAuthorityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:authority:{p.ClusterReference}:{p.AuthorityName}");
        return Dispatch(new EstablishAuthorityCommand(id, p.ClusterReference, p.AuthorityName), "authority_established", "structural.cluster.authority.establish_failed", ct);
    }

    [HttpPost("establish-with-parent")]
    public Task<IActionResult> EstablishWithParent([FromBody] ApiRequest<EstablishAuthorityWithParentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:authority:{p.ClusterReference}:{p.AuthorityName}:{p.EffectiveAt.UtcTicks}");
        return Dispatch(new EstablishAuthorityWithParentCommand(id, p.ClusterReference, p.AuthorityName, p.EffectiveAt), "authority_established_with_parent", "structural.cluster.authority.establish_with_parent_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateAuthorityRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateAuthorityCommand(request.Data.AuthorityId), "authority_activated", "structural.cluster.authority.activate_failed", ct);

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<RevokeAuthorityRequestModel> request, CancellationToken ct)
        => Dispatch(new RevokeAuthorityCommand(request.Data.AuthorityId), "authority_revoked", "structural.cluster.authority.revoke_failed", ct);

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendAuthorityRequestModel> request, CancellationToken ct)
        => Dispatch(new SuspendAuthorityCommand(request.Data.AuthorityId), "authority_suspended", "structural.cluster.authority.suspend_failed", ct);

    [HttpPost("reactivate")]
    public Task<IActionResult> Reactivate([FromBody] ApiRequest<ReactivateAuthorityRequestModel> request, CancellationToken ct)
        => Dispatch(new ReactivateAuthorityCommand(request.Data.AuthorityId), "authority_reactivated", "structural.cluster.authority.reactivate_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireAuthorityRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireAuthorityCommand(request.Data.AuthorityId), "authority_retired", "structural.cluster.authority.retire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_authority.authority_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.authority.not_found", $"Authority {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<AuthorityReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize AuthorityReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, AuthorityRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record EstablishAuthorityRequestModel(Guid ClusterReference, string AuthorityName);
public sealed record EstablishAuthorityWithParentRequestModel(Guid ClusterReference, string AuthorityName, DateTimeOffset EffectiveAt);
public sealed record ActivateAuthorityRequestModel(Guid AuthorityId);
public sealed record RevokeAuthorityRequestModel(Guid AuthorityId);
public sealed record SuspendAuthorityRequestModel(Guid AuthorityId);
public sealed record ReactivateAuthorityRequestModel(Guid AuthorityId);
public sealed record RetireAuthorityRequestModel(Guid AuthorityId);
