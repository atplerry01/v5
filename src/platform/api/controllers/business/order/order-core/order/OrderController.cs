using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Order.OrderCore.Order;

[Authorize]
[ApiController]
[Route("api/order-core/order")]
[ApiExplorerSettings(GroupName = "business.order.order-core.order")]
public sealed class OrderController : BusinessControllerBase
{
    private static readonly DomainRoute OrderRoute = new("business", "order", "order");

    public OrderController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateOrderRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateOrderCommand(p.OrderId, p.SourceReferenceId, p.Description);
        return Dispatch(cmd, OrderRoute, "order_created", "business.order.order.create_failed", ct);
    }

    [HttpPost("confirm")]
    public Task<IActionResult> Confirm([FromBody] ApiRequest<OrderIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ConfirmOrderCommand(request.Data.OrderId);
        return Dispatch(cmd, OrderRoute, "order_confirmed", "business.order.order.confirm_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<OrderIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new CompleteOrderCommand(request.Data.OrderId);
        return Dispatch(cmd, OrderRoute, "order_completed", "business.order.order.complete_failed", ct);
    }

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<OrderIdRequestModel> request, CancellationToken ct)
    {
        // CancelledAt is read from IClock (host-bound wall clock in production)
        // so it flows through the event-sourced invariant without leaking
        // DateTimeOffset.UtcNow into controller code.
        var cmd = new CancelOrderCommand(request.Data.OrderId, Clock.UtcNow);
        return Dispatch(cmd, OrderRoute, "order_cancelled", "business.order.order.cancel_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetOrder(Guid id, CancellationToken ct) =>
        LoadReadModel<OrderReadModel>(
            id,
            "projection_business_order_order",
            "order_read_model",
            "business.order.order.not_found",
            ct);
}

public sealed record CreateOrderRequestModel(
    Guid OrderId,
    Guid SourceReferenceId,
    string Description);

public sealed record OrderIdRequestModel(Guid OrderId);
