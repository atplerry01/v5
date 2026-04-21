using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Entitlement.UsageControl.Limit;

[Authorize]
[ApiController]
[Route("api/usage-control/limit")]
[ApiExplorerSettings(GroupName = "business.entitlement.usage-control.limit")]
public sealed class LimitController : BusinessControllerBase
{
    private static readonly DomainRoute LimitRoute = new("business", "entitlement", "limit");

    public LimitController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateLimitRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateLimitCommand(p.LimitId, p.SubjectId, p.ThresholdValue);
        return Dispatch(cmd, LimitRoute, "limit_created", "business.entitlement.limit.create_failed", ct);
    }

    [HttpPost("enforce")]
    public Task<IActionResult> Enforce([FromBody] ApiRequest<LimitIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new EnforceLimitCommand(request.Data.LimitId);
        return Dispatch(cmd, LimitRoute, "limit_enforced", "business.entitlement.limit.enforce_failed", ct);
    }

    [HttpPost("breach")]
    public Task<IActionResult> Breach([FromBody] ApiRequest<BreachLimitRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new BreachLimitCommand(p.LimitId, p.ObservedValue);
        return Dispatch(cmd, LimitRoute, "limit_breached", "business.entitlement.limit.breach_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetLimit(Guid id, CancellationToken ct) =>
        LoadReadModel<LimitReadModel>(
            id,
            "projection_business_entitlement_limit",
            "limit_read_model",
            "business.entitlement.limit.not_found",
            ct);
}

public sealed record CreateLimitRequestModel(Guid LimitId, Guid SubjectId, int ThresholdValue);
public sealed record LimitIdRequestModel(Guid LimitId);
public sealed record BreachLimitRequestModel(Guid LimitId, int ObservedValue);
