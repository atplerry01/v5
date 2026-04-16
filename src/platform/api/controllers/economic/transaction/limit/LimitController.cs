using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Transaction.Limit;

[Authorize]
[ApiController]
[Route("api/limit")]
[ApiExplorerSettings(GroupName = "economic.transaction.limit")]
public sealed class LimitController : ControllerBase
{
    private static readonly DomainRoute LimitRoute = new("economic", "transaction", "limit");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public LimitController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/limit/define ───────────────────────────────────

    [HttpPost("define")]
    public async Task<IActionResult> Define(
        [FromBody] ApiRequest<DefineLimitRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var limitId = _idGenerator.Generate(
            $"economic:transaction:limit:{p.AccountId}:{p.Type}:{p.Threshold}:{p.Currency}");

        var command = new DefineLimitCommand(
            limitId,
            p.AccountId,
            p.Type,
            p.Threshold,
            p.Currency,
            now);

        var result = await _dispatcher.DispatchAsync(command, LimitRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new { limitId }, now))
            : BadRequest(ApiResponse.Fail("economic.limit.define_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/limit/{id}/check ───────────────────────────────

    [HttpPost("{limitId:guid}/check")]
    public async Task<IActionResult> Check(
        Guid limitId,
        [FromBody] ApiRequest<CheckLimitRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var command = new CheckLimitCommand(limitId, p.TransactionId, p.TransactionAmount, now);

        var result = await _dispatcher.DispatchAsync(command, LimitRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("limit_checked"), now))
            : BadRequest(ApiResponse.Fail("economic.limit.check_failed", result.Error ?? "Unknown error", now));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record DefineLimitRequestModel(
    Guid AccountId,
    string Type,
    decimal Threshold,
    string Currency);

public sealed record CheckLimitRequestModel(
    Guid TransactionId,
    decimal TransactionAmount);
