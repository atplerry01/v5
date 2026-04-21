using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Customer.SegmentationAndLifecycle.ContactPoint;

[Authorize]
[ApiController]
[Route("api/segmentation-and-lifecycle/contact-point")]
[ApiExplorerSettings(GroupName = "business.customer.segmentation-and-lifecycle.contact-point")]
public sealed class ContactPointController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a.
    private static readonly DomainRoute ContactPointRoute = new("business", "customer", "contact-point");

    public ContactPointController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateContactPointRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateContactPointCommand(p.ContactPointId, p.CustomerId, p.Kind, p.Value);
        return Dispatch(cmd, ContactPointRoute, "contact_point_created", "business.customer.contact-point.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateContactPointRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateContactPointCommand(p.ContactPointId, p.Value);
        return Dispatch(cmd, ContactPointRoute, "contact_point_updated", "business.customer.contact-point.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ContactPointIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateContactPointCommand(request.Data.ContactPointId);
        return Dispatch(cmd, ContactPointRoute, "contact_point_activated", "business.customer.contact-point.activate_failed", ct);
    }

    [HttpPost("set-preferred")]
    public Task<IActionResult> SetPreferred([FromBody] ApiRequest<SetContactPointPreferredRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SetContactPointPreferredCommand(p.ContactPointId, p.IsPreferred);
        return Dispatch(cmd, ContactPointRoute, "contact_point_preferred_set", "business.customer.contact-point.set_preferred_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ContactPointIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveContactPointCommand(request.Data.ContactPointId);
        return Dispatch(cmd, ContactPointRoute, "contact_point_archived", "business.customer.contact-point.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetContactPoint(Guid id, CancellationToken ct) =>
        LoadReadModel<ContactPointReadModel>(
            id,
            "projection_business_customer_contact_point",
            "contact_point_read_model",
            "business.customer.contact-point.not_found",
            ct);
}

public sealed record CreateContactPointRequestModel(Guid ContactPointId, Guid CustomerId, string Kind, string Value);
public sealed record UpdateContactPointRequestModel(Guid ContactPointId, string Value);
public sealed record SetContactPointPreferredRequestModel(Guid ContactPointId, bool IsPreferred);
public sealed record ContactPointIdRequestModel(Guid ContactPointId);
