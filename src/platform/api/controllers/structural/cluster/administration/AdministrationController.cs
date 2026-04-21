using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Structural.Cluster.Administration;

[Authorize]
[ApiController]
[Route("api/structural/cluster/administration")]
[ApiExplorerSettings(GroupName = "structural.cluster.administration")]
public sealed class AdministrationController : ControllerBase
{
    private static readonly DomainRoute AdministrationRoute = new("structural", "cluster", "administration");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public AdministrationController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("establish")]
    public Task<IActionResult> Establish([FromBody] ApiRequest<EstablishAdministrationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:administration:{p.ClusterReference}:{p.AdministrationName}");
        return Dispatch(new EstablishAdministrationCommand(id, p.ClusterReference, p.AdministrationName), "administration_established", "structural.cluster.administration.establish_failed", ct);
    }

    [HttpPost("establish-with-parent")]
    public Task<IActionResult> EstablishWithParent([FromBody] ApiRequest<EstablishAdministrationWithParentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"structural:cluster:administration:{p.ClusterReference}:{p.AdministrationName}:{p.EffectiveAt.UtcTicks}");
        return Dispatch(new EstablishAdministrationWithParentCommand(id, p.ClusterReference, p.AdministrationName, p.EffectiveAt), "administration_established_with_parent", "structural.cluster.administration.establish_with_parent_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateAdministrationRequestModel> request, CancellationToken ct)
        => Dispatch(new ActivateAdministrationCommand(request.Data.AdministrationId), "administration_activated", "structural.cluster.administration.activate_failed", ct);

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<SuspendAdministrationRequestModel> request, CancellationToken ct)
        => Dispatch(new SuspendAdministrationCommand(request.Data.AdministrationId), "administration_suspended", "structural.cluster.administration.suspend_failed", ct);

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<RetireAdministrationRequestModel> request, CancellationToken ct)
        => Dispatch(new RetireAdministrationCommand(request.Data.AdministrationId), "administration_retired", "structural.cluster.administration.retire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_structural_cluster_administration.administration_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("structural.cluster.administration.not_found", $"Administration {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<AdministrationReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize AdministrationReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, AdministrationRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record EstablishAdministrationRequestModel(Guid ClusterReference, string AdministrationName);
public sealed record EstablishAdministrationWithParentRequestModel(Guid ClusterReference, string AdministrationName, DateTimeOffset EffectiveAt);
public sealed record ActivateAdministrationRequestModel(Guid AdministrationId);
public sealed record SuspendAdministrationRequestModel(Guid AdministrationId);
public sealed record RetireAdministrationRequestModel(Guid AdministrationId);
