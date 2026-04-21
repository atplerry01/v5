using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Content.Streaming.DeliveryGovernance.Access;

[Authorize]
[ApiController]
[Route("api/content/streaming/delivery-governance/access")]
[ApiExplorerSettings(GroupName = "content.streaming.delivery_governance.access")]
public sealed class StreamAccessController : ControllerBase
{
    private static readonly DomainRoute AccessRoute = new("content", "streaming", "access");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _connStr;

    public StreamAccessController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock, IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _connStr = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("grant")]
    public Task<IActionResult> Grant([FromBody] ApiRequest<GrantStreamAccessRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        return Dispatch(new GrantStreamAccessCommand(p.AccessId, p.StreamId, p.Mode, p.WindowStart, p.WindowEnd, p.Token, _clock.UtcNow),
            "stream_access_granted", "content.streaming.delivery_governance.access.grant_failed", ct);
    }

    [HttpPost("restrict")]
    public Task<IActionResult> Restrict([FromBody] ApiRequest<RestrictStreamAccessRequestModel> request, CancellationToken ct)
        => Dispatch(new RestrictStreamAccessCommand(request.Data.AccessId, request.Data.Reason, _clock.UtcNow),
            "stream_access_restricted", "content.streaming.delivery_governance.access.restrict_failed", ct);

    [HttpPost("unrestrict")]
    public Task<IActionResult> Unrestrict([FromBody] ApiRequest<UnrestrictStreamAccessRequestModel> request, CancellationToken ct)
        => Dispatch(new UnrestrictStreamAccessCommand(request.Data.AccessId, _clock.UtcNow),
            "stream_access_unrestricted", "content.streaming.delivery_governance.access.unrestrict_failed", ct);

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<RevokeStreamAccessRequestModel> request, CancellationToken ct)
        => Dispatch(new RevokeStreamAccessCommand(request.Data.AccessId, request.Data.Reason, _clock.UtcNow),
            "stream_access_revoked", "content.streaming.delivery_governance.access.revoke_failed", ct);

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<ExpireStreamAccessRequestModel> request, CancellationToken ct)
        => Dispatch(new ExpireStreamAccessCommand(request.Data.AccessId, _clock.UtcNow),
            "stream_access_expired", "content.streaming.delivery_governance.access.expire_failed", ct);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccess(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT state FROM projection_content_streaming_delivery_governance_access.stream_access_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("content.streaming.delivery_governance.access.not_found", $"StreamAccess {id} not found.", _clock.UtcNow));
        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<StreamAccessReadModel>(stateJson)
            ?? throw new InvalidOperationException($"Failed to deserialize StreamAccessReadModel for aggregate {id}.");
        return Ok(ApiResponse.Ok(model, Guid.Empty, _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, AccessRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record GrantStreamAccessRequestModel(Guid AccessId, Guid StreamId, string Mode, DateTimeOffset WindowStart, DateTimeOffset WindowEnd, string Token);
public sealed record RestrictStreamAccessRequestModel(Guid AccessId, string Reason);
public sealed record UnrestrictStreamAccessRequestModel(Guid AccessId);
public sealed record RevokeStreamAccessRequestModel(Guid AccessId, string Reason);
public sealed record ExpireStreamAccessRequestModel(Guid AccessId);
