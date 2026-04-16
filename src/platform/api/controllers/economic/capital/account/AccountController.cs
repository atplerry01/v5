using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Capital.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Capital.Account;

[Authorize]
[ApiController]
[Route("api/capital/account")]
[ApiExplorerSettings(GroupName = "economic.capital.account")]
public sealed class AccountController : CapitalControllerBase
{
    private static readonly DomainRoute AccountRoute = new("economic", "capital", "account");

    public AccountController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("open")]
    public Task<IActionResult> OpenAccount([FromBody] ApiRequest<OpenAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var accountId = IdGenerator.Generate($"economic:capital:account:{p.OwnerId}:{p.Currency}");
        var cmd = new OpenCapitalAccountCommand(accountId, p.OwnerId, p.Currency, Clock.UtcNow);
        return Dispatch(cmd, AccountRoute, "capital_account_opened", "economic.capital.account.open_failed", ct);
    }

    [HttpPost("credit")]
    public Task<IActionResult> Credit([FromBody] ApiRequest<CreditAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FundCapitalAccountCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_credited", "economic.capital.account.credit_failed", ct);
    }

    [HttpPost("debit")]
    public Task<IActionResult> Debit([FromBody] ApiRequest<DebitAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AllocateCapitalAccountCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_debited", "economic.capital.account.debit_failed", ct);
    }

    [HttpPost("reserve")]
    public Task<IActionResult> ReserveAccount([FromBody] ApiRequest<ReserveAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReserveCapitalAccountCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_reserved", "economic.capital.account.reserve_failed", ct);
    }

    [HttpPost("release")]
    public Task<IActionResult> ReleaseAccount([FromBody] ApiRequest<ReleaseAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseCapitalReservationCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_released", "economic.capital.account.release_failed", ct);
    }

    [HttpPost("freeze")]
    public Task<IActionResult> FreezeAccount([FromBody] ApiRequest<FreezeAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FreezeCapitalAccountCommand(p.AccountId, p.Reason);
        return Dispatch(cmd, AccountRoute, "capital_account_frozen", "economic.capital.account.freeze_failed", ct);
    }

    [HttpPost("close")]
    public Task<IActionResult> CloseAccount([FromBody] ApiRequest<CloseAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CloseCapitalAccountCommand(p.AccountId, Clock.UtcNow);
        return Dispatch(cmd, AccountRoute, "capital_account_closed", "economic.capital.account.close_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAccount(Guid id, CancellationToken ct) =>
        LoadReadModel<CapitalAccountReadModel>(
            id,
            "projection_economic_capital_account",
            "capital_account_read_model",
            "economic.capital.account.not_found",
            ct);
}

public sealed record OpenAccountRequestModel(Guid OwnerId, string Currency);
public sealed record CreditAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record DebitAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record ReserveAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record ReleaseAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record FreezeAccountRequestModel(Guid AccountId, string Reason);
public sealed record CloseAccountRequestModel(Guid AccountId);
