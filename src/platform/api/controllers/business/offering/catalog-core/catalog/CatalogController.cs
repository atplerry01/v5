using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Offering.CatalogCore.Catalog;

[Authorize]
[ApiController]
[Route("api/catalog-core/catalog")]
[ApiExplorerSettings(GroupName = "business.offering.catalog-core.catalog")]
public sealed class CatalogController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute CatalogRoute = new("business", "offering", "catalog");

    public CatalogController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateCatalogRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // CatalogId MUST be supplied by the caller per DET-SEED-DERIVATION-01.
        var cmd = new CreateCatalogCommand(p.CatalogId, p.Name, p.Category);
        return Dispatch(cmd, CatalogRoute, "catalog_created", "business.offering.catalog.create_failed", ct);
    }

    [HttpPost("add-member")]
    public Task<IActionResult> AddMember([FromBody] ApiRequest<CatalogMemberMutationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddCatalogMemberCommand(p.CatalogId, p.MemberId, p.MemberKind);
        return Dispatch(cmd, CatalogRoute, "catalog_member_added", "business.offering.catalog.add_member_failed", ct);
    }

    [HttpPost("remove-member")]
    public Task<IActionResult> RemoveMember([FromBody] ApiRequest<CatalogMemberMutationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveCatalogMemberCommand(p.CatalogId, p.MemberId, p.MemberKind);
        return Dispatch(cmd, CatalogRoute, "catalog_member_removed", "business.offering.catalog.remove_member_failed", ct);
    }

    [HttpPost("publish")]
    public Task<IActionResult> Publish([FromBody] ApiRequest<CatalogIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new PublishCatalogCommand(request.Data.CatalogId);
        return Dispatch(cmd, CatalogRoute, "catalog_published", "business.offering.catalog.publish_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<CatalogIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveCatalogCommand(request.Data.CatalogId);
        return Dispatch(cmd, CatalogRoute, "catalog_archived", "business.offering.catalog.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetCatalog(Guid id, CancellationToken ct) =>
        LoadReadModel<CatalogReadModel>(
            id,
            "projection_business_offering_catalog",
            "catalog_read_model",
            "business.offering.catalog.not_found",
            ct);
}

public sealed record CreateCatalogRequestModel(Guid CatalogId, string Name, string Category);
public sealed record CatalogMemberMutationRequestModel(Guid CatalogId, Guid MemberId, string MemberKind);
public sealed record CatalogIdRequestModel(Guid CatalogId);
