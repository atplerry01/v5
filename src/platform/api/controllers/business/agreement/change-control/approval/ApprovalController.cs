using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.ChangeControl.Approval;

[Authorize]
[ApiController]
[Route("api/change-control/approval")]
[ApiExplorerSettings(GroupName = "business.agreement.change-control.approval")]
public sealed class ApprovalController : BusinessControllerBase
{
    private static readonly DomainRoute ApprovalRoute = new("business", "agreement", "approval");

    public ApprovalController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateApprovalRequestModel> request, CancellationToken ct)
    {
        var cmd = new CreateApprovalCommand(request.Data.ApprovalId);
        return Dispatch(cmd, ApprovalRoute, "approval_created", "business.agreement.approval.create_failed", ct);
    }

    [HttpPost("approve")]
    public Task<IActionResult> Approve([FromBody] ApiRequest<ApprovalIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ApproveApprovalCommand(request.Data.ApprovalId);
        return Dispatch(cmd, ApprovalRoute, "approval_approved", "business.agreement.approval.approve_failed", ct);
    }

    [HttpPost("reject")]
    public Task<IActionResult> Reject([FromBody] ApiRequest<ApprovalIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RejectApprovalCommand(request.Data.ApprovalId);
        return Dispatch(cmd, ApprovalRoute, "approval_rejected", "business.agreement.approval.reject_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetApproval(Guid id, CancellationToken ct) =>
        LoadReadModel<ApprovalReadModel>(
            id,
            "projection_business_agreement_approval",
            "approval_read_model",
            "business.agreement.approval.not_found",
            ct);
}

public sealed record CreateApprovalRequestModel(Guid ApprovalId);
public sealed record ApprovalIdRequestModel(Guid ApprovalId);
