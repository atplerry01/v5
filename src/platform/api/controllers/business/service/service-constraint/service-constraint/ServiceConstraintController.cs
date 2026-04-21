using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Business;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Service.ServiceConstraint.ServiceConstraint;

[Authorize]
[ApiController]
[Route("api/service-constraint/service-constraint")]
[ApiExplorerSettings(GroupName = "business.service.service-constraint.service-constraint")]
public sealed class ServiceConstraintController : BusinessControllerBase
{
    private static readonly DomainRoute ServiceConstraintRoute = new("business", "service", "service-constraint");

    public ServiceConstraintController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateServiceConstraintRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateServiceConstraintCommand(p.ServiceConstraintId, p.ServiceDefinitionId, p.Kind, p.Descriptor);
        return Dispatch(cmd, ServiceConstraintRoute, "service_constraint_created", "business.service.service-constraint.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateServiceConstraintRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateServiceConstraintCommand(p.ServiceConstraintId, p.Kind, p.Descriptor);
        return Dispatch(cmd, ServiceConstraintRoute, "service_constraint_updated", "business.service.service-constraint.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ServiceConstraintIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateServiceConstraintCommand(request.Data.ServiceConstraintId);
        return Dispatch(cmd, ServiceConstraintRoute, "service_constraint_activated", "business.service.service-constraint.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ServiceConstraintIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveServiceConstraintCommand(request.Data.ServiceConstraintId);
        return Dispatch(cmd, ServiceConstraintRoute, "service_constraint_archived", "business.service.service-constraint.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetServiceConstraint(Guid id, CancellationToken ct) =>
        LoadReadModel<ServiceConstraintReadModel>(
            id,
            "projection_business_service_service_constraint",
            "service_constraint_read_model",
            "business.service.service-constraint.not_found",
            ct);
}

public sealed record CreateServiceConstraintRequestModel(
    Guid ServiceConstraintId,
    Guid ServiceDefinitionId,
    int Kind,
    string Descriptor);

public sealed record UpdateServiceConstraintRequestModel(
    Guid ServiceConstraintId,
    int Kind,
    string Descriptor);

public sealed record ServiceConstraintIdRequestModel(Guid ServiceConstraintId);
