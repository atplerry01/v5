using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Customer.IdentityAndProfile.Account;

[Authorize]
[ApiController]
[Route("api/identity-and-profile/account")]
[ApiExplorerSettings(GroupName = "business.customer.identity-and-profile.account")]
public sealed class AccountController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute AccountRoute = new("business", "customer", "account");

    public AccountController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateAccountCommand(p.AccountId, p.CustomerId, p.Name, p.Type);
        return Dispatch(cmd, AccountRoute, "account_created", "business.customer.account.create_failed", ct);
    }

    [HttpPost("rename")]
    public Task<IActionResult> Rename([FromBody] ApiRequest<RenameAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RenameAccountCommand(p.AccountId, p.Name);
        return Dispatch(cmd, AccountRoute, "account_renamed", "business.customer.account.rename_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<AccountIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateAccountCommand(request.Data.AccountId);
        return Dispatch(cmd, AccountRoute, "account_activated", "business.customer.account.activate_failed", ct);
    }

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<AccountIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SuspendAccountCommand(request.Data.AccountId);
        return Dispatch(cmd, AccountRoute, "account_suspended", "business.customer.account.suspend_failed", ct);
    }

    [HttpPost("close")]
    public Task<IActionResult> Close([FromBody] ApiRequest<AccountIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new CloseAccountCommand(request.Data.AccountId);
        return Dispatch(cmd, AccountRoute, "account_closed", "business.customer.account.close_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAccount(Guid id, CancellationToken ct) =>
        LoadReadModel<AccountReadModel>(
            id,
            "projection_business_customer_account",
            "account_read_model",
            "business.customer.account.not_found",
            ct);
}

public sealed record CreateAccountRequestModel(Guid AccountId, Guid CustomerId, string Name, string Type);
public sealed record RenameAccountRequestModel(Guid AccountId, string Name);
public sealed record AccountIdRequestModel(Guid AccountId);
