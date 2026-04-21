using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.ChangeControl.Amendment;

[Authorize]
[ApiController]
[Route("api/change-control/amendment")]
[ApiExplorerSettings(GroupName = "business.agreement.change-control.amendment")]
public sealed class AmendmentController : BusinessControllerBase
{
    private static readonly DomainRoute AmendmentRoute = new("business", "agreement", "amendment");

    public AmendmentController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateAmendmentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateAmendmentCommand(p.AmendmentId, p.TargetId);
        return Dispatch(cmd, AmendmentRoute, "amendment_created", "business.agreement.amendment.create_failed", ct);
    }

    [HttpPost("apply")]
    public Task<IActionResult> Apply([FromBody] ApiRequest<AmendmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ApplyAmendmentCommand(request.Data.AmendmentId);
        return Dispatch(cmd, AmendmentRoute, "amendment_applied", "business.agreement.amendment.apply_failed", ct);
    }

    [HttpPost("revert")]
    public Task<IActionResult> Revert([FromBody] ApiRequest<AmendmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevertAmendmentCommand(request.Data.AmendmentId);
        return Dispatch(cmd, AmendmentRoute, "amendment_reverted", "business.agreement.amendment.revert_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAmendment(Guid id, CancellationToken ct) =>
        LoadReadModel<AmendmentReadModel>(
            id,
            "projection_business_agreement_amendment",
            "amendment_read_model",
            "business.agreement.amendment.not_found",
            ct);
}

public sealed record CreateAmendmentRequestModel(Guid AmendmentId, Guid TargetId);
public sealed record AmendmentIdRequestModel(Guid AmendmentId);
