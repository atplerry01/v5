using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Transaction.Charge;

[Authorize]
[ApiController]
[Route("api/charge")]
[ApiExplorerSettings(GroupName = "economic.transaction.charge")]
public sealed class ChargeController : ControllerBase
{
    private static readonly DomainRoute ChargeRoute = new("economic", "transaction", "charge");

    // Domain enum ChargeType mirror — validate at boundary so invalid values return 400
    // instead of surfacing an ArgumentException through the global exception handler.
    private static readonly HashSet<string> ValidChargeTypes = new(StringComparer.Ordinal)
    {
        "Fixed", "Percentage",
    };

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public ChargeController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/charge/calculate ───────────────────────────────

    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate(
        [FromBody] ApiRequest<CalculateChargeRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        if (!ValidChargeTypes.Contains(p.Type))
            return BadRequest(ApiResponse.Fail(
                "economic.charge.invalid_type",
                $"Unknown charge type: '{p.Type}'. Valid values: {string.Join(", ", ValidChargeTypes)}.",
                now));

        var chargeId = _idGenerator.Generate(
            $"economic:transaction:charge:{p.TransactionId}:{p.Type}:{p.BaseAmount}:{p.ChargeAmount}:{p.Currency}");

        var command = new CalculateChargeCommand(
            chargeId,
            p.TransactionId,
            p.Type,
            p.BaseAmount,
            p.ChargeAmount,
            p.Currency,
            now);

        var result = await _dispatcher.DispatchAsync(command, ChargeRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new { chargeId }, now))
            : BadRequest(ApiResponse.Fail("economic.charge.calculate_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/charge/{id}/apply ──────────────────────────────

    [HttpPost("{chargeId:guid}/apply")]
    public async Task<IActionResult> Apply(
        Guid chargeId,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var command = new ApplyChargeCommand(chargeId, now);

        var result = await _dispatcher.DispatchAsync(command, ChargeRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("charge_applied"), now))
            : BadRequest(ApiResponse.Fail("economic.charge.apply_failed", result.Error ?? "Unknown error", now));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record CalculateChargeRequestModel(
    Guid TransactionId,
    string Type,
    decimal BaseAmount,
    decimal ChargeAmount,
    string Currency);
