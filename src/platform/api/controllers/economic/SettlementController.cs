using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic;

[Authorize]
[ApiController]
[Route("api/settlement")]
[ApiExplorerSettings(GroupName = "economic.transaction.settlement")]
public sealed class SettlementController : ControllerBase
{
    private static readonly DomainRoute SettlementRoute = new("economic", "transaction", "settlement");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public SettlementController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/settlement/initiate ────────────────────────────

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateSettlement(
        [FromBody] ApiRequest<InitiateSettlementRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var settlementId = _idGenerator.Generate(
            $"economic:transaction:settlement:{p.SourceReference}:{p.Amount}:{p.Currency}:{p.Provider}");

        var command = new InitiateSettlementCommand(
            settlementId,
            p.Amount,
            p.Currency,
            p.SourceReference,
            p.Provider);

        var result = await _dispatcher.DispatchAsync(command, SettlementRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("settlement_initiated"), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.settlement.initiate_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    // ── POST /api/settlement/complete ────────────────────────────

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteSettlement(
        [FromBody] ApiRequest<CompleteSettlementRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var command = new CompleteSettlementCommand(p.SettlementId, p.ExternalReferenceId);

        var result = await _dispatcher.DispatchAsync(command, SettlementRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("settlement_completed"), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.settlement.complete_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    // ── POST /api/settlement/fail ────────────────────────────────

    [HttpPost("fail")]
    public async Task<IActionResult> FailSettlement(
        [FromBody] ApiRequest<FailSettlementRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var command = new FailSettlementCommand(p.SettlementId, p.Reason);

        var result = await _dispatcher.DispatchAsync(command, SettlementRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("settlement_failed"), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.settlement.fail_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record InitiateSettlementRequestModel(
    decimal Amount,
    string Currency,
    string SourceReference,
    string Provider);

public sealed record CompleteSettlementRequestModel(
    Guid SettlementId,
    string ExternalReferenceId);

public sealed record FailSettlementRequestModel(
    Guid SettlementId,
    string Reason);
