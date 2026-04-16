using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Ledger.Treasury;

[Authorize]
[ApiController]
[Route("api/economic/ledger/treasury")]
[ApiExplorerSettings(GroupName = "economic.ledger.treasury")]
public sealed class TreasuryController : ControllerBase
{
    private static readonly DomainRoute TreasuryRoute = new("economic", "ledger", "treasury");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public TreasuryController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTreasury(
        [FromBody] ApiRequest<CreateTreasuryRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var treasuryId = _idGenerator.Generate(
            $"economic:ledger:treasury:{p.Currency}:{p.Reference}");

        var command = new CreateTreasuryCommand(treasuryId, p.Currency);

        var result = await _dispatcher.DispatchAsync(command, TreasuryRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("treasury_created"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.treasury.create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("allocate")]
    public async Task<IActionResult> AllocateFunds(
        [FromBody] ApiRequest<AllocateFundsRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var command = new AllocateFundsCommand(p.TreasuryId, p.Amount);

        var result = await _dispatcher.DispatchAsync(command, TreasuryRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("treasury_funds_allocated"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.treasury.allocate_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("release")]
    public async Task<IActionResult> ReleaseFunds(
        [FromBody] ApiRequest<ReleaseFundsRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var command = new ReleaseFundsCommand(p.TreasuryId, p.Amount);

        var result = await _dispatcher.DispatchAsync(command, TreasuryRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("treasury_funds_released"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.treasury.release_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

public sealed record CreateTreasuryRequestModel(
    string Currency,
    string Reference);

public sealed record AllocateFundsRequestModel(
    Guid TreasuryId,
    decimal Amount);

public sealed record ReleaseFundsRequestModel(
    Guid TreasuryId,
    decimal Amount);
