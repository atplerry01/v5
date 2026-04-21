using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Customer.SegmentationAndLifecycle.Segment;

[Authorize]
[ApiController]
[Route("api/segmentation-and-lifecycle/segment")]
[ApiExplorerSettings(GroupName = "business.customer.segmentation-and-lifecycle.segment")]
public sealed class SegmentController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a.
    private static readonly DomainRoute SegmentRoute = new("business", "customer", "segment");

    public SegmentController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateSegmentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateSegmentCommand(p.SegmentId, p.Code, p.Name, p.Type, p.Criteria);
        return Dispatch(cmd, SegmentRoute, "segment_created", "business.customer.segment.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateSegmentRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateSegmentCommand(p.SegmentId, p.Name, p.Criteria);
        return Dispatch(cmd, SegmentRoute, "segment_updated", "business.customer.segment.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<SegmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateSegmentCommand(request.Data.SegmentId);
        return Dispatch(cmd, SegmentRoute, "segment_activated", "business.customer.segment.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<SegmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveSegmentCommand(request.Data.SegmentId);
        return Dispatch(cmd, SegmentRoute, "segment_archived", "business.customer.segment.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetSegment(Guid id, CancellationToken ct) =>
        LoadReadModel<SegmentReadModel>(
            id,
            "projection_business_customer_segment",
            "segment_read_model",
            "business.customer.segment.not_found",
            ct);
}

public sealed record CreateSegmentRequestModel(Guid SegmentId, string Code, string Name, string Type, string Criteria);
public sealed record UpdateSegmentRequestModel(Guid SegmentId, string Name, string Criteria);
public sealed record SegmentIdRequestModel(Guid SegmentId);
