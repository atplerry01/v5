using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Offering.CommercialShape.Package;

[Authorize]
[ApiController]
[Route("api/commercial-shape/package")]
[ApiExplorerSettings(GroupName = "business.offering.commercial-shape.package")]
public sealed class PackageController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute PackageRoute = new("business", "offering", "package");

    public PackageController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreatePackageRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // PackageId MUST be supplied by the caller per DET-SEED-DERIVATION-01 —
        // no clock/Random entropy on the API hot path.
        var cmd = new CreatePackageCommand(p.PackageId, p.Code, p.Name);
        return Dispatch(cmd, PackageRoute, "package_created", "business.offering.package.create_failed", ct);
    }

    [HttpPost("add-member")]
    public Task<IActionResult> AddMember([FromBody] ApiRequest<PackageMemberRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddPackageMemberCommand(p.PackageId, p.MemberKind, p.MemberId);
        return Dispatch(cmd, PackageRoute, "package_member_added", "business.offering.package.add_member_failed", ct);
    }

    [HttpPost("remove-member")]
    public Task<IActionResult> RemoveMember([FromBody] ApiRequest<PackageMemberRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemovePackageMemberCommand(p.PackageId, p.MemberKind, p.MemberId);
        return Dispatch(cmd, PackageRoute, "package_member_removed", "business.offering.package.remove_member_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<PackageIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivatePackageCommand(request.Data.PackageId);
        return Dispatch(cmd, PackageRoute, "package_activated", "business.offering.package.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<PackageIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchivePackageCommand(request.Data.PackageId);
        return Dispatch(cmd, PackageRoute, "package_archived", "business.offering.package.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetPackage(Guid id, CancellationToken ct) =>
        LoadReadModel<PackageReadModel>(
            id,
            "projection_business_offering_package",
            "package_read_model",
            "business.offering.package.not_found",
            ct);
}

public sealed record CreatePackageRequestModel(Guid PackageId, string Code, string Name);
public sealed record PackageMemberRequestModel(Guid PackageId, string MemberKind, Guid MemberId);
public sealed record PackageIdRequestModel(Guid PackageId);
