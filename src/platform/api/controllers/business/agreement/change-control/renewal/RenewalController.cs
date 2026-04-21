using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.ChangeControl.Renewal;

[Authorize]
[ApiController]
[Route("api/change-control/renewal")]
[ApiExplorerSettings(GroupName = "business.agreement.change-control.renewal")]
public sealed class RenewalController : BusinessControllerBase
{
    private static readonly DomainRoute RenewalRoute = new("business", "agreement", "renewal");

    public RenewalController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateRenewalRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateRenewalCommand(p.RenewalId, p.SourceId);
        return Dispatch(cmd, RenewalRoute, "renewal_created", "business.agreement.renewal.create_failed", ct);
    }

    [HttpPost("renew")]
    public Task<IActionResult> Renew([FromBody] ApiRequest<RenewalIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RenewRenewalCommand(request.Data.RenewalId);
        return Dispatch(cmd, RenewalRoute, "renewal_renewed", "business.agreement.renewal.renew_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<RenewalIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireRenewalCommand(request.Data.RenewalId);
        return Dispatch(cmd, RenewalRoute, "renewal_expired", "business.agreement.renewal.expire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetRenewal(Guid id, CancellationToken ct) =>
        LoadReadModel<RenewalReadModel>(
            id,
            "projection_business_agreement_renewal",
            "renewal_read_model",
            "business.agreement.renewal.not_found",
            ct);
}

public sealed record CreateRenewalRequestModel(Guid RenewalId, Guid SourceId);
public sealed record RenewalIdRequestModel(Guid RenewalId);
