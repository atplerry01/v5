using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Provider.ProviderCore.ProviderTier;

[Authorize]
[ApiController]
[Route("api/provider-core/provider-tier")]
[ApiExplorerSettings(GroupName = "business.provider.provider-core.provider-tier")]
public sealed class ProviderTierController : BusinessControllerBase
{
    private static readonly DomainRoute ProviderTierRoute = new("business", "provider", "provider-tier");

    public ProviderTierController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateProviderTierRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateProviderTierCommand(p.ProviderTierId, p.Code, p.Name, p.Rank);
        return Dispatch(cmd, ProviderTierRoute, "provider_tier_created", "business.provider.provider-tier.create_failed", ct);
    }

    [HttpPost("update")]
    public Task<IActionResult> Update([FromBody] ApiRequest<UpdateProviderTierRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateProviderTierCommand(p.ProviderTierId, p.Name, p.Rank);
        return Dispatch(cmd, ProviderTierRoute, "provider_tier_updated", "business.provider.provider-tier.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ProviderTierIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateProviderTierCommand(request.Data.ProviderTierId);
        return Dispatch(cmd, ProviderTierRoute, "provider_tier_activated", "business.provider.provider-tier.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ProviderTierIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveProviderTierCommand(request.Data.ProviderTierId);
        return Dispatch(cmd, ProviderTierRoute, "provider_tier_archived", "business.provider.provider-tier.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProviderTier(Guid id, CancellationToken ct) =>
        LoadReadModel<ProviderTierReadModel>(
            id,
            "projection_business_provider_provider_tier",
            "provider_tier_read_model",
            "business.provider.provider-tier.not_found",
            ct);
}

public sealed record CreateProviderTierRequestModel(
    Guid ProviderTierId,
    string Code,
    string Name,
    int Rank);

public sealed record UpdateProviderTierRequestModel(
    Guid ProviderTierId,
    string Name,
    int Rank);

public sealed record ProviderTierIdRequestModel(Guid ProviderTierId);
