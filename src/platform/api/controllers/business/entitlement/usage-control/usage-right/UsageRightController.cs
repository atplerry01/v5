using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Entitlement.UsageControl.UsageRight;

[Authorize]
[ApiController]
[Route("api/usage-control/usage-right")]
[ApiExplorerSettings(GroupName = "business.entitlement.usage-control.usage-right")]
public sealed class UsageRightController : BusinessControllerBase
{
    private static readonly DomainRoute UsageRightRoute = new("business", "entitlement", "usage-right");

    public UsageRightController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateUsageRightRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateUsageRightCommand(p.UsageRightId, p.SubjectId, p.ReferenceId, p.TotalUnits);
        return Dispatch(cmd, UsageRightRoute, "usage_right_created", "business.entitlement.usage_right.create_failed", ct);
    }

    [HttpPost("use")]
    public Task<IActionResult> Use([FromBody] ApiRequest<UseUsageRightRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UseUsageRightCommand(p.UsageRightId, p.RecordId, p.UnitsUsed);
        return Dispatch(cmd, UsageRightRoute, "usage_right_used", "business.entitlement.usage_right.use_failed", ct);
    }

    [HttpPost("consume")]
    public Task<IActionResult> Consume([FromBody] ApiRequest<UsageRightIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ConsumeUsageRightCommand(request.Data.UsageRightId);
        return Dispatch(cmd, UsageRightRoute, "usage_right_consumed", "business.entitlement.usage_right.consume_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetUsageRight(Guid id, CancellationToken ct) =>
        LoadReadModel<UsageRightReadModel>(
            id,
            "projection_business_entitlement_usage_right",
            "usage_right_read_model",
            "business.entitlement.usage_right.not_found",
            ct);
}

public sealed record CreateUsageRightRequestModel(Guid UsageRightId, Guid SubjectId, Guid ReferenceId, int TotalUnits);
public sealed record UseUsageRightRequestModel(Guid UsageRightId, Guid RecordId, int UnitsUsed);
public sealed record UsageRightIdRequestModel(Guid UsageRightId);
