using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Trust.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Trust.Identity.Registry;

[Authorize]
[ApiController]
[Route("api/trust/identity/registry")]
[ApiExplorerSettings(GroupName = "trust.identity.registry")]
public sealed class RegistryController : TrustControllerBase
{
    private static readonly DomainRoute RegistryRoute = new("trust", "identity", "registry");

    public RegistryController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("initiate")]
    public Task<IActionResult> Initiate([FromBody] ApiRequest<InitiateRegistrationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var registryId = IdGenerator.Generate($"trust:identity:registry:{p.Email}:{p.RegistrationType}");
        var cmd = new InitiateRegistrationCommand(registryId, p.Email, p.RegistrationType, Clock.UtcNow);
        return Dispatch(cmd, RegistryRoute, "registration_initiated", "trust.identity.registry.initiate_failed", ct);
    }

    [HttpPost("verify")]
    public Task<IActionResult> Verify([FromBody] ApiRequest<RegistryIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new VerifyRegistrationCommand(request.Data.RegistryId);
        return Dispatch(cmd, RegistryRoute, "registration_verified", "trust.identity.registry.verify_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<RegistryIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateRegistrationCommand(request.Data.RegistryId);
        return Dispatch(cmd, RegistryRoute, "registration_activated", "trust.identity.registry.activate_failed", ct);
    }

    [HttpPost("reject")]
    public Task<IActionResult> Reject([FromBody] ApiRequest<RejectRegistrationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RejectRegistrationCommand(p.RegistryId, p.Reason);
        return Dispatch(cmd, RegistryRoute, "registration_rejected", "trust.identity.registry.reject_failed", ct);
    }

    [HttpPost("lock")]
    public Task<IActionResult> Lock([FromBody] ApiRequest<LockRegistrationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new LockRegistrationCommand(p.RegistryId, p.Reason);
        return Dispatch(cmd, RegistryRoute, "registration_locked", "trust.identity.registry.lock_failed", ct);
    }

    [HttpGet("{registryId:guid}")]
    public Task<IActionResult> Get(Guid registryId, CancellationToken ct)
        => LoadReadModel<RegistryReadModel>(registryId, "projection_trust_identity_registry", "registry_read_model", "trust.identity.registry.not_found", ct);
}

public sealed record InitiateRegistrationRequestModel(string Email, string RegistrationType);
public sealed record RegistryIdRequestModel(Guid RegistryId);
public sealed record RejectRegistrationRequestModel(Guid RegistryId, string Reason);
public sealed record LockRegistrationRequestModel(Guid RegistryId, string Reason);
