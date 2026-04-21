using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Order.OrderChange.Cancellation;

[Authorize]
[ApiController]
[Route("api/order-change/cancellation")]
[ApiExplorerSettings(GroupName = "business.order.order-change.cancellation")]
public sealed class CancellationController : BusinessControllerBase
{
    private static readonly DomainRoute CancellationRoute = new("business", "order", "cancellation");

    public CancellationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("request")]
    public new Task<IActionResult> Request([FromBody] ApiRequest<RequestCancellationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RequestCancellationCommand(p.CancellationId, p.OrderId, p.Reason, Clock.UtcNow);
        return Dispatch(cmd, CancellationRoute, "cancellation_requested", "business.order.cancellation.request_failed", ct);
    }

    [HttpPost("confirm")]
    public Task<IActionResult> Confirm([FromBody] ApiRequest<CancellationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ConfirmCancellationCommand(request.Data.CancellationId, Clock.UtcNow);
        return Dispatch(cmd, CancellationRoute, "cancellation_confirmed", "business.order.cancellation.confirm_failed", ct);
    }

    [HttpPost("reject")]
    public Task<IActionResult> Reject([FromBody] ApiRequest<CancellationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RejectCancellationCommand(request.Data.CancellationId, Clock.UtcNow);
        return Dispatch(cmd, CancellationRoute, "cancellation_rejected", "business.order.cancellation.reject_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetCancellation(Guid id, CancellationToken ct) =>
        LoadReadModel<CancellationReadModel>(
            id,
            "projection_business_order_cancellation",
            "cancellation_read_model",
            "business.order.cancellation.not_found",
            ct);
}

public sealed record RequestCancellationRequestModel(Guid CancellationId, Guid OrderId, string Reason);
public sealed record CancellationIdRequestModel(Guid CancellationId);
