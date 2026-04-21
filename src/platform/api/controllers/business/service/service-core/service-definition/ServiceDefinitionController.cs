using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Service.ServiceCore.ServiceDefinition;

[Authorize]
[ApiController]
[Route("api/service-core/service-definition")]
[ApiExplorerSettings(GroupName = "business.service.service-core.service-definition")]
public sealed class ServiceDefinitionController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ServiceDefinitionRoute = new("business", "service", "service-definition");

    public ServiceDefinitionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateServiceDefinitionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ServiceDefinitionId MUST be supplied by the caller so the dispatch seed derives
        // only from stable command coordinates (DET-SEED-DERIVATION-01 — no
        // clock/Random entropy on the API hot path).
        var cmd = new CreateServiceDefinitionCommand(p.ServiceDefinitionId, p.Name, p.Category);
        return Dispatch(cmd, ServiceDefinitionRoute, "service_definition_created", "business.service.service-definition.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateServiceDefinitionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateServiceDefinitionCommand(p.ServiceDefinitionId, p.Name, p.Category);
        return Dispatch(cmd, ServiceDefinitionRoute, "service_definition_updated", "business.service.service-definition.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ServiceDefinitionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateServiceDefinitionCommand(request.Data.ServiceDefinitionId);
        return Dispatch(cmd, ServiceDefinitionRoute, "service_definition_activated", "business.service.service-definition.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ServiceDefinitionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveServiceDefinitionCommand(request.Data.ServiceDefinitionId);
        return Dispatch(cmd, ServiceDefinitionRoute, "service_definition_archived", "business.service.service-definition.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetServiceDefinition(Guid id, CancellationToken ct) =>
        LoadReadModel<ServiceDefinitionReadModel>(
            id,
            "projection_business_service_service_definition",
            "service_definition_read_model",
            "business.service.service-definition.not_found",
            ct);
}

public sealed record CreateServiceDefinitionRequestModel(Guid ServiceDefinitionId, string Name, string Category);
public sealed record UpdateServiceDefinitionRequestModel(Guid ServiceDefinitionId, string Name, string Category);
public sealed record ServiceDefinitionIdRequestModel(Guid ServiceDefinitionId);
