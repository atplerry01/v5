using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Provider.ProviderCore.ProviderCapability;

[Authorize]
[ApiController]
[Route("api/provider-core/provider-capability")]
[ApiExplorerSettings(GroupName = "business.provider.provider-core.provider-capability")]
public sealed class ProviderCapabilityController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) — the
    // physical four-level path business/provider/provider-core/provider-capability
    // projects onto this tuple.
    private static readonly DomainRoute ProviderCapabilityRoute = new("business", "provider", "provider-capability");

    public ProviderCapabilityController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateProviderCapabilityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateProviderCapabilityCommand(p.ProviderCapabilityId, p.ProviderId, p.Code, p.Name);
        return Dispatch(cmd, ProviderCapabilityRoute, "provider_capability_created", "business.provider.provider-capability.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateProviderCapabilityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateProviderCapabilityCommand(p.ProviderCapabilityId, p.Name);
        return Dispatch(cmd, ProviderCapabilityRoute, "provider_capability_updated", "business.provider.provider-capability.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ProviderCapabilityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateProviderCapabilityCommand(request.Data.ProviderCapabilityId);
        return Dispatch(cmd, ProviderCapabilityRoute, "provider_capability_activated", "business.provider.provider-capability.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ProviderCapabilityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveProviderCapabilityCommand(request.Data.ProviderCapabilityId);
        return Dispatch(cmd, ProviderCapabilityRoute, "provider_capability_archived", "business.provider.provider-capability.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProviderCapability(Guid id, CancellationToken ct) =>
        LoadReadModel<ProviderCapabilityReadModel>(
            id,
            "projection_business_provider_provider_capability",
            "provider_capability_read_model",
            "business.provider.provider-capability.not_found",
            ct);
}

public sealed record CreateProviderCapabilityRequestModel(
    Guid ProviderCapabilityId,
    Guid ProviderId,
    string Code,
    string Name);

public sealed record UpdateProviderCapabilityRequestModel(
    Guid ProviderCapabilityId,
    string Name);

public sealed record ProviderCapabilityIdRequestModel(Guid ProviderCapabilityId);
