using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Service.ServiceCore.ServiceLevel;

[Authorize]
[ApiController]
[Route("api/service-core/service-level")]
[ApiExplorerSettings(GroupName = "business.service.service-core.service-level")]
public sealed class ServiceLevelController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ServiceLevelRoute = new("business", "service", "service-level");

    public ServiceLevelController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateServiceLevelRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ServiceLevelId MUST be supplied by the caller (DET-SEED-DERIVATION-01).
        var cmd = new CreateServiceLevelCommand(p.ServiceLevelId, p.ServiceDefinitionId, p.Code, p.Name, p.Target);
        return Dispatch(cmd, ServiceLevelRoute, "service_level_created", "business.service.service-level.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateServiceLevelRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateServiceLevelCommand(p.ServiceLevelId, p.Name, p.Target);
        return Dispatch(cmd, ServiceLevelRoute, "service_level_updated", "business.service.service-level.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ServiceLevelIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateServiceLevelCommand(request.Data.ServiceLevelId);
        return Dispatch(cmd, ServiceLevelRoute, "service_level_activated", "business.service.service-level.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ServiceLevelIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveServiceLevelCommand(request.Data.ServiceLevelId);
        return Dispatch(cmd, ServiceLevelRoute, "service_level_archived", "business.service.service-level.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetServiceLevel(Guid id, CancellationToken ct) =>
        LoadReadModel<ServiceLevelReadModel>(
            id,
            "projection_business_service_service_level",
            "service_level_read_model",
            "business.service.service-level.not_found",
            ct);
}

public sealed record CreateServiceLevelRequestModel(Guid ServiceLevelId, Guid ServiceDefinitionId, string Code, string Name, string Target);
public sealed record UpdateServiceLevelRequestModel(Guid ServiceLevelId, string Name, string Target);
public sealed record ServiceLevelIdRequestModel(Guid ServiceLevelId);
