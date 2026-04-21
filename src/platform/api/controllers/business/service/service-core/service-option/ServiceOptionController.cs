using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Service.ServiceCore.ServiceOption;

[Authorize]
[ApiController]
[Route("api/service-core/service-option")]
[ApiExplorerSettings(GroupName = "business.service.service-core.service-option")]
public sealed class ServiceOptionController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ServiceOptionRoute = new("business", "service", "service-option");

    public ServiceOptionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateServiceOptionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ServiceOptionId MUST be supplied by the caller (DET-SEED-DERIVATION-01).
        var cmd = new CreateServiceOptionCommand(p.ServiceOptionId, p.ServiceDefinitionId, p.Code, p.Name, p.Kind);
        return Dispatch(cmd, ServiceOptionRoute, "service_option_created", "business.service.service-option.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateServiceOptionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateServiceOptionCommand(p.ServiceOptionId, p.Name, p.Kind);
        return Dispatch(cmd, ServiceOptionRoute, "service_option_updated", "business.service.service-option.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ServiceOptionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateServiceOptionCommand(request.Data.ServiceOptionId);
        return Dispatch(cmd, ServiceOptionRoute, "service_option_activated", "business.service.service-option.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ServiceOptionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveServiceOptionCommand(request.Data.ServiceOptionId);
        return Dispatch(cmd, ServiceOptionRoute, "service_option_archived", "business.service.service-option.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetServiceOption(Guid id, CancellationToken ct) =>
        LoadReadModel<ServiceOptionReadModel>(
            id,
            "projection_business_service_service_option",
            "service_option_read_model",
            "business.service.service-option.not_found",
            ct);
}

public sealed record CreateServiceOptionRequestModel(Guid ServiceOptionId, Guid ServiceDefinitionId, string Code, string Name, string Kind);
public sealed record UpdateServiceOptionRequestModel(Guid ServiceOptionId, string Name, string Kind);
public sealed record ServiceOptionIdRequestModel(Guid ServiceOptionId);
