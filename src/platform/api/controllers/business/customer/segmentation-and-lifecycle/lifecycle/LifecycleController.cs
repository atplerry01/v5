using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Customer.SegmentationAndLifecycle.Lifecycle;

[Authorize]
[ApiController]
[Route("api/segmentation-and-lifecycle/lifecycle")]
[ApiExplorerSettings(GroupName = "business.customer.segmentation-and-lifecycle.lifecycle")]
public sealed class LifecycleController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a.
    private static readonly DomainRoute LifecycleRoute = new("business", "customer", "lifecycle");

    public LifecycleController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("start")]
    public Task<IActionResult> Start([FromBody] ApiRequest<StartLifecycleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new StartLifecycleCommand(p.LifecycleId, p.CustomerId, p.InitialStage, Clock.UtcNow);
        return Dispatch(cmd, LifecycleRoute, "lifecycle_started", "business.customer.lifecycle.start_failed", ct);
    }

    [HttpPost("change-stage")]
    public Task<IActionResult> ChangeStage([FromBody] ApiRequest<ChangeLifecycleStageRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ChangeLifecycleStageCommand(p.LifecycleId, p.To, Clock.UtcNow);
        return Dispatch(cmd, LifecycleRoute, "lifecycle_stage_changed", "business.customer.lifecycle.change_stage_failed", ct);
    }

    [HttpPost("close")]
    public Task<IActionResult> Close([FromBody] ApiRequest<LifecycleIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new CloseLifecycleCommand(request.Data.LifecycleId, Clock.UtcNow);
        return Dispatch(cmd, LifecycleRoute, "lifecycle_closed", "business.customer.lifecycle.close_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetLifecycle(Guid id, CancellationToken ct) =>
        LoadReadModel<LifecycleReadModel>(
            id,
            "projection_business_customer_lifecycle",
            "lifecycle_read_model",
            "business.customer.lifecycle.not_found",
            ct);
}

public sealed record StartLifecycleRequestModel(Guid LifecycleId, Guid CustomerId, string InitialStage);
public sealed record ChangeLifecycleStageRequestModel(Guid LifecycleId, string To);
public sealed record LifecycleIdRequestModel(Guid LifecycleId);
