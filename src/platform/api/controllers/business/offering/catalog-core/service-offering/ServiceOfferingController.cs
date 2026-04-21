using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Offering.CatalogCore.ServiceOffering;

[Authorize]
[ApiController]
[Route("api/catalog-core/service-offering")]
[ApiExplorerSettings(GroupName = "business.offering.catalog-core.service-offering")]
public sealed class ServiceOfferingController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ServiceOfferingRoute = new("business", "offering", "service-offering");

    public ServiceOfferingController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateServiceOfferingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ServiceOfferingId MUST be supplied by the caller per DET-SEED-DERIVATION-01.
        var cmd = new CreateServiceOfferingCommand(
            p.ServiceOfferingId,
            p.Name,
            p.ServiceDefinitionId,
            p.PackageId);
        return Dispatch(cmd, ServiceOfferingRoute, "service_offering_created", "business.offering.service-offering.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateServiceOfferingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateServiceOfferingCommand(p.ServiceOfferingId, p.Name);
        return Dispatch(cmd, ServiceOfferingRoute, "service_offering_updated", "business.offering.service-offering.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ServiceOfferingIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateServiceOfferingCommand(request.Data.ServiceOfferingId);
        return Dispatch(cmd, ServiceOfferingRoute, "service_offering_activated", "business.offering.service-offering.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ServiceOfferingIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveServiceOfferingCommand(request.Data.ServiceOfferingId);
        return Dispatch(cmd, ServiceOfferingRoute, "service_offering_archived", "business.offering.service-offering.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetServiceOffering(Guid id, CancellationToken ct) =>
        LoadReadModel<ServiceOfferingReadModel>(
            id,
            "projection_business_offering_service_offering",
            "service_offering_read_model",
            "business.offering.service-offering.not_found",
            ct);
}

public sealed record CreateServiceOfferingRequestModel(
    Guid ServiceOfferingId,
    string Name,
    Guid ServiceDefinitionId,
    Guid? PackageId);
public sealed record UpdateServiceOfferingRequestModel(Guid ServiceOfferingId, string Name);
public sealed record ServiceOfferingIdRequestModel(Guid ServiceOfferingId);
