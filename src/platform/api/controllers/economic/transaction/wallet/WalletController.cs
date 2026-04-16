using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Transaction.Wallet;

[Authorize]
[ApiController]
[Route("api/wallet")]
[ApiExplorerSettings(GroupName = "economic.transaction.wallet")]
public sealed class WalletController : ControllerBase
{
    private static readonly DomainRoute WalletRoute = new("economic", "transaction", "wallet");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public WalletController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/wallet/create ──────────────────────────────────

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] ApiRequest<CreateWalletRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var walletId = _idGenerator.Generate(
            $"economic:transaction:wallet:{p.OwnerId}:{p.AccountId}");

        var command = new CreateWalletCommand(walletId, p.OwnerId, p.AccountId, now);

        var result = await _dispatcher.DispatchAsync(command, WalletRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new { walletId }, now))
            : BadRequest(ApiResponse.Fail("economic.wallet.create_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/wallet/{id}/request-transaction ────────────────

    [HttpPost("{walletId:guid}/request-transaction")]
    public async Task<IActionResult> RequestTransaction(
        Guid walletId,
        [FromBody] ApiRequest<RequestWalletTransactionRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var command = new RequestWalletTransactionCommand(
            walletId, p.DestinationAccountId, p.Amount, p.Currency, now);

        var result = await _dispatcher.DispatchAsync(command, WalletRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("wallet_transaction_requested"), now))
            : BadRequest(ApiResponse.Fail("economic.wallet.request_transaction_failed", result.Error ?? "Unknown error", now));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record CreateWalletRequestModel(
    Guid OwnerId,
    Guid AccountId);

public sealed record RequestWalletTransactionRequestModel(
    Guid DestinationAccountId,
    decimal Amount,
    string Currency);
