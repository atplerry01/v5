using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Order.OrderChange.Amendment;

[Authorize]
[ApiController]
[Route("api/order-change/amendment")]
[ApiExplorerSettings(GroupName = "business.order.order-change.amendment")]
public sealed class AmendmentController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a.
    private static readonly DomainRoute AmendmentRoute = new("business", "order", "amendment");

    public AmendmentController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("request")]
    public new Task<IActionResult> Request([FromBody] ApiRequest<RequestAmendmentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RequestAmendmentCommand(p.AmendmentId, p.OrderId, p.Reason, Clock.UtcNow);
        return Dispatch(cmd, AmendmentRoute, "amendment_requested", "business.order.amendment.request_failed", ct);
    }

    [HttpPost("accept")]
    public Task<IActionResult> Accept([FromBody] ApiRequest<AmendmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new AcceptAmendmentCommand(request.Data.AmendmentId, Clock.UtcNow);
        return Dispatch(cmd, AmendmentRoute, "amendment_accepted", "business.order.amendment.accept_failed", ct);
    }

    [HttpPost("apply")]
    public Task<IActionResult> Apply([FromBody] ApiRequest<AmendmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ApplyAmendmentCommand(request.Data.AmendmentId, Clock.UtcNow);
        return Dispatch(cmd, AmendmentRoute, "amendment_applied", "business.order.amendment.apply_failed", ct);
    }

    [HttpPost("reject")]
    public Task<IActionResult> Reject([FromBody] ApiRequest<AmendmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RejectAmendmentCommand(request.Data.AmendmentId, Clock.UtcNow);
        return Dispatch(cmd, AmendmentRoute, "amendment_rejected", "business.order.amendment.reject_failed", ct);
    }

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<AmendmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new CancelAmendmentCommand(request.Data.AmendmentId, Clock.UtcNow);
        return Dispatch(cmd, AmendmentRoute, "amendment_cancelled", "business.order.amendment.cancel_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAmendment(Guid id, CancellationToken ct) =>
        LoadReadModel<AmendmentReadModel>(
            id,
            "projection_business_order_amendment",
            "amendment_read_model",
            "business.order.amendment.not_found",
            ct);
}

public sealed record RequestAmendmentRequestModel(Guid AmendmentId, Guid OrderId, string Reason);
public sealed record AmendmentIdRequestModel(Guid AmendmentId);
