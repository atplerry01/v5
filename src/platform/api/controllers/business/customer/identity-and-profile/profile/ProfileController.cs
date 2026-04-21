using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Customer.IdentityAndProfile.Profile;

[Authorize]
[ApiController]
[Route("api/identity-and-profile/profile")]
[ApiExplorerSettings(GroupName = "business.customer.identity-and-profile.profile")]
public sealed class ProfileController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ProfileRoute = new("business", "customer", "profile");

    public ProfileController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateProfileRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateProfileCommand(p.ProfileId, p.CustomerId, p.DisplayName);
        return Dispatch(cmd, ProfileRoute, "profile_created", "business.customer.profile.create_failed", ct);
    }

    [HttpPost("rename")]
    public Task<IActionResult> Rename([FromBody] ApiRequest<RenameProfileRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RenameProfileCommand(p.ProfileId, p.DisplayName);
        return Dispatch(cmd, ProfileRoute, "profile_renamed", "business.customer.profile.rename_failed", ct);
    }

    [HttpPost("set-descriptor")]
    public Task<IActionResult> SetDescriptor([FromBody] ApiRequest<SetProfileDescriptorRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SetProfileDescriptorCommand(p.ProfileId, p.Key, p.Value);
        return Dispatch(cmd, ProfileRoute, "profile_descriptor_set", "business.customer.profile.set_descriptor_failed", ct);
    }

    [HttpPost("remove-descriptor")]
    public Task<IActionResult> RemoveDescriptor([FromBody] ApiRequest<RemoveProfileDescriptorRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveProfileDescriptorCommand(p.ProfileId, p.Key);
        return Dispatch(cmd, ProfileRoute, "profile_descriptor_removed", "business.customer.profile.remove_descriptor_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ProfileIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateProfileCommand(request.Data.ProfileId);
        return Dispatch(cmd, ProfileRoute, "profile_activated", "business.customer.profile.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ProfileIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveProfileCommand(request.Data.ProfileId);
        return Dispatch(cmd, ProfileRoute, "profile_archived", "business.customer.profile.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProfile(Guid id, CancellationToken ct) =>
        LoadReadModel<ProfileReadModel>(
            id,
            "projection_business_customer_profile",
            "profile_read_model",
            "business.customer.profile.not_found",
            ct);
}

public sealed record CreateProfileRequestModel(Guid ProfileId, Guid CustomerId, string DisplayName);
public sealed record RenameProfileRequestModel(Guid ProfileId, string DisplayName);
public sealed record SetProfileDescriptorRequestModel(Guid ProfileId, string Key, string Value);
public sealed record RemoveProfileDescriptorRequestModel(Guid ProfileId, string Key);
public sealed record ProfileIdRequestModel(Guid ProfileId);
