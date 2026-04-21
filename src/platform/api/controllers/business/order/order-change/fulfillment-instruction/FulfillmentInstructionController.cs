using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Order.OrderChange.FulfillmentInstruction;

[Authorize]
[ApiController]
[Route("api/order-change/fulfillment-instruction")]
[ApiExplorerSettings(GroupName = "business.order.order-change.fulfillment-instruction")]
public sealed class FulfillmentInstructionController : BusinessControllerBase
{
    private static readonly DomainRoute FulfillmentInstructionRoute = new("business", "order", "fulfillment-instruction");

    public FulfillmentInstructionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateFulfillmentInstructionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateFulfillmentInstructionCommand(p.FulfillmentInstructionId, p.OrderId, p.Directive, p.LineItemId);
        return Dispatch(cmd, FulfillmentInstructionRoute,
            "fulfillment_instruction_created",
            "business.order.fulfillment-instruction.create_failed", ct);
    }

    [HttpPost("issue")]
    public Task<IActionResult> Issue([FromBody] ApiRequest<FulfillmentInstructionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new IssueFulfillmentInstructionCommand(request.Data.FulfillmentInstructionId, Clock.UtcNow);
        return Dispatch(cmd, FulfillmentInstructionRoute,
            "fulfillment_instruction_issued",
            "business.order.fulfillment-instruction.issue_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<FulfillmentInstructionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new CompleteFulfillmentInstructionCommand(request.Data.FulfillmentInstructionId, Clock.UtcNow);
        return Dispatch(cmd, FulfillmentInstructionRoute,
            "fulfillment_instruction_completed",
            "business.order.fulfillment-instruction.complete_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<FulfillmentInstructionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeFulfillmentInstructionCommand(request.Data.FulfillmentInstructionId, Clock.UtcNow);
        return Dispatch(cmd, FulfillmentInstructionRoute,
            "fulfillment_instruction_revoked",
            "business.order.fulfillment-instruction.revoke_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetFulfillmentInstruction(Guid id, CancellationToken ct) =>
        LoadReadModel<FulfillmentInstructionReadModel>(
            id,
            "projection_business_order_fulfillment_instruction",
            "fulfillment_instruction_read_model",
            "business.order.fulfillment-instruction.not_found",
            ct);
}

public sealed record CreateFulfillmentInstructionRequestModel(Guid FulfillmentInstructionId, Guid OrderId, string Directive, Guid? LineItemId);
public sealed record FulfillmentInstructionIdRequestModel(Guid FulfillmentInstructionId);
