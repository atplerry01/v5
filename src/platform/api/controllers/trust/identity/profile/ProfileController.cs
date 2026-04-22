using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Trust.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Trust.Identity.Profile;

[Authorize]
[ApiController]
[Route("api/trust/identity/profile")]
[ApiExplorerSettings(GroupName = "trust.identity.profile")]
public sealed class ProfileController : TrustControllerBase
{
    private static readonly DomainRoute ProfileRoute = new("trust", "identity", "profile");

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
        var profileId = IdGenerator.Generate($"trust:identity:profile:{p.IdentityReference}:{p.ProfileType}");
        var cmd = new CreateProfileCommand(profileId, p.IdentityReference, p.DisplayName, p.ProfileType, Clock.UtcNow);
        return Dispatch(cmd, ProfileRoute, "profile_created", "trust.identity.profile.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ProfileIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateProfileCommand(request.Data.ProfileId);
        return Dispatch(cmd, ProfileRoute, "profile_activated", "trust.identity.profile.activate_failed", ct);
    }

    [HttpPost("deactivate")]
    public Task<IActionResult> Deactivate([FromBody] ApiRequest<ProfileIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeactivateProfileCommand(request.Data.ProfileId);
        return Dispatch(cmd, ProfileRoute, "profile_deactivated", "trust.identity.profile.deactivate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProfile(Guid id, CancellationToken ct) =>
        LoadReadModel<ProfileReadModel>(
            id,
            "projection_trust_identity_profile",
            "profile_read_model",
            "trust.identity.profile.not_found",
            ct);
}

public sealed record CreateProfileRequestModel(Guid IdentityReference, string DisplayName, string ProfileType);
public sealed record ProfileIdRequestModel(Guid ProfileId);
