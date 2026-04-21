using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Business;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Service.ServiceConstraint.ServiceWindow;

[Authorize]
[ApiController]
[Route("api/service-constraint/service-window")]
[ApiExplorerSettings(GroupName = "business.service.service-constraint.service-window")]
public sealed class ServiceWindowController : BusinessControllerBase
{
    private static readonly DomainRoute ServiceWindowRoute = new("business", "service", "service-window");

    public ServiceWindowController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateServiceWindowRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateServiceWindowCommand(p.ServiceWindowId, p.ServiceDefinitionId, p.StartsAt, p.EndsAt);
        return Dispatch(cmd, ServiceWindowRoute, "service_window_created", "business.service.service-window.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateServiceWindowRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateServiceWindowCommand(p.ServiceWindowId, p.StartsAt, p.EndsAt);
        return Dispatch(cmd, ServiceWindowRoute, "service_window_updated", "business.service.service-window.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ServiceWindowIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateServiceWindowCommand(request.Data.ServiceWindowId);
        return Dispatch(cmd, ServiceWindowRoute, "service_window_activated", "business.service.service-window.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ServiceWindowIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveServiceWindowCommand(request.Data.ServiceWindowId);
        return Dispatch(cmd, ServiceWindowRoute, "service_window_archived", "business.service.service-window.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetServiceWindow(Guid id, CancellationToken ct) =>
        LoadReadModel<ServiceWindowReadModel>(
            id,
            "projection_business_service_service_window",
            "service_window_read_model",
            "business.service.service-window.not_found",
            ct);
}

public sealed record CreateServiceWindowRequestModel(
    Guid ServiceWindowId,
    Guid ServiceDefinitionId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);

public sealed record UpdateServiceWindowRequestModel(
    Guid ServiceWindowId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);

public sealed record ServiceWindowIdRequestModel(Guid ServiceWindowId);
