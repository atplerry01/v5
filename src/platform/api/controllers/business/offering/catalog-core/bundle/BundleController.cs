using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Offering.CatalogCore.Bundle;

[Authorize]
[ApiController]
[Route("api/catalog-core/bundle")]
[ApiExplorerSettings(GroupName = "business.offering.catalog-core.bundle")]
public sealed class BundleController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute BundleRoute = new("business", "offering", "bundle");

    public BundleController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateBundleRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // BundleId MUST be supplied by the caller per DET-SEED-DERIVATION-01.
        var cmd = new CreateBundleCommand(p.BundleId, p.Name);
        return Dispatch(cmd, BundleRoute, "bundle_created", "business.offering.bundle.create_failed", ct);
    }

    [HttpPost("add-member")]
    public Task<IActionResult> AddMember([FromBody] ApiRequest<BundleMemberMutationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddBundleMemberCommand(p.BundleId, p.MemberId, p.MemberKind);
        return Dispatch(cmd, BundleRoute, "bundle_member_added", "business.offering.bundle.add_member_failed", ct);
    }

    [HttpPost("remove-member")]
    public Task<IActionResult> RemoveMember([FromBody] ApiRequest<BundleMemberMutationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveBundleMemberCommand(p.BundleId, p.MemberId, p.MemberKind);
        return Dispatch(cmd, BundleRoute, "bundle_member_removed", "business.offering.bundle.remove_member_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<BundleIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateBundleCommand(request.Data.BundleId);
        return Dispatch(cmd, BundleRoute, "bundle_activated", "business.offering.bundle.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<BundleIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveBundleCommand(request.Data.BundleId);
        return Dispatch(cmd, BundleRoute, "bundle_archived", "business.offering.bundle.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetBundle(Guid id, CancellationToken ct) =>
        LoadReadModel<BundleReadModel>(
            id,
            "projection_business_offering_bundle",
            "bundle_read_model",
            "business.offering.bundle.not_found",
            ct);
}

public sealed record CreateBundleRequestModel(Guid BundleId, string Name);
public sealed record BundleMemberMutationRequestModel(Guid BundleId, Guid MemberId, string MemberKind);
public sealed record BundleIdRequestModel(Guid BundleId);
