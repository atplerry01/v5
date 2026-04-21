using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Order.OrderCore.LineItem;

[Authorize]
[ApiController]
[Route("api/order-core/line-item")]
[ApiExplorerSettings(GroupName = "business.order.order-core.line-item")]
public sealed class LineItemController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute LineItemRoute = new("business", "order", "line-item");

    public LineItemController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateLineItemRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateLineItemCommand(
            p.LineItemId,
            p.OrderId,
            p.SubjectKind,
            p.SubjectId,
            p.QuantityValue,
            p.QuantityUnit);
        return Dispatch(cmd, LineItemRoute, "line_item_created", "business.order.line-item.create_failed", ct);
    }

    [HttpPost("update-quantity")]
    public Task<IActionResult> UpdateQuantity([FromBody] ApiRequest<UpdateLineItemQuantityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateLineItemQuantityCommand(p.LineItemId, p.QuantityValue, p.QuantityUnit);
        return Dispatch(cmd, LineItemRoute, "line_item_updated", "business.order.line-item.update_failed", ct);
    }

    [HttpPost("cancel")]
    public Task<IActionResult> Cancel([FromBody] ApiRequest<LineItemIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new CancelLineItemCommand(request.Data.LineItemId);
        return Dispatch(cmd, LineItemRoute, "line_item_cancelled", "business.order.line-item.cancel_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetLineItem(Guid id, CancellationToken ct) =>
        LoadReadModel<LineItemReadModel>(
            id,
            "projection_business_order_line_item",
            "line_item_read_model",
            "business.order.line-item.not_found",
            ct);
}

public sealed record CreateLineItemRequestModel(
    Guid LineItemId,
    Guid OrderId,
    int SubjectKind,
    Guid SubjectId,
    decimal QuantityValue,
    string QuantityUnit);

public sealed record UpdateLineItemQuantityRequestModel(
    Guid LineItemId,
    decimal QuantityValue,
    string QuantityUnit);

public sealed record LineItemIdRequestModel(Guid LineItemId);
