using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Ledger.Obligation;

[Authorize]
[ApiController]
[Route("api/economic/ledger/obligation")]
[ApiExplorerSettings(GroupName = "economic.ledger.obligation")]
public sealed class ObligationController : ControllerBase
{
    private static readonly DomainRoute ObligationRoute = new("economic", "ledger", "obligation");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public ObligationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateObligation(
        [FromBody] ApiRequest<CreateObligationRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var obligationId = _idGenerator.Generate(
            $"economic:ledger:obligation:{p.CounterpartyId}:{p.Type}:{p.Amount}:{p.Currency}:{p.Reference}");

        var command = new CreateObligationCommand(
            obligationId,
            p.CounterpartyId,
            p.Type,
            p.Amount,
            p.Currency);

        var result = await _dispatcher.DispatchAsync(command, ObligationRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("obligation_created"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.obligation.create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("fulfil")]
    public async Task<IActionResult> FulfilObligation(
        [FromBody] ApiRequest<FulfilObligationRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var command = new FulfilObligationCommand(p.ObligationId, p.SettlementId);

        var result = await _dispatcher.DispatchAsync(command, ObligationRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("obligation_fulfilled"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.obligation.fulfil_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelObligation(
        [FromBody] ApiRequest<CancelObligationRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var command = new CancelObligationCommand(p.ObligationId, p.Reason);

        var result = await _dispatcher.DispatchAsync(command, ObligationRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("obligation_cancelled"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.obligation.cancel_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

public sealed record CreateObligationRequestModel(
    Guid CounterpartyId,
    string Type,
    decimal Amount,
    string Currency,
    string Reference);

public sealed record FulfilObligationRequestModel(
    Guid ObligationId,
    Guid SettlementId);

public sealed record CancelObligationRequestModel(
    Guid ObligationId,
    string Reason);
