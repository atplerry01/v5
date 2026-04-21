using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Provider.ProviderScope.ProviderAvailability;

[Authorize]
[ApiController]
[Route("api/provider-scope/provider-availability")]
[ApiExplorerSettings(GroupName = "business.provider.provider-scope.provider-availability")]
public sealed class ProviderAvailabilityController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ProviderAvailabilityRoute = new("business", "provider", "provider-availability");

    public ProviderAvailabilityController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateProviderAvailabilityRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ProviderAvailabilityId MUST be supplied by the caller so the dispatch
        // seed derives only from stable command coordinates per DET-SEED-DERIVATION-01.
        var cmd = new CreateProviderAvailabilityCommand(p.ProviderAvailabilityId, p.ProviderId, p.StartsAt, p.EndsAt);
        return Dispatch(cmd, ProviderAvailabilityRoute, "provider_availability_created", "business.provider.provider-availability.create_failed", ct);
    }

    [HttpPost("update-window")]
    public Task<IActionResult> UpdateWindow([FromBody] ApiRequest<UpdateProviderAvailabilityWindowRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new UpdateProviderAvailabilityWindowCommand(p.ProviderAvailabilityId, p.StartsAt, p.EndsAt);
        return Dispatch(cmd, ProviderAvailabilityRoute, "provider_availability_updated", "business.provider.provider-availability.update_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ProviderAvailabilityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateProviderAvailabilityCommand(request.Data.ProviderAvailabilityId);
        return Dispatch(cmd, ProviderAvailabilityRoute, "provider_availability_activated", "business.provider.provider-availability.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ProviderAvailabilityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveProviderAvailabilityCommand(request.Data.ProviderAvailabilityId);
        return Dispatch(cmd, ProviderAvailabilityRoute, "provider_availability_archived", "business.provider.provider-availability.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProviderAvailability(Guid id, CancellationToken ct) =>
        LoadReadModel<ProviderAvailabilityReadModel>(
            id,
            "projection_business_provider_provider_availability",
            "provider_availability_read_model",
            "business.provider.provider-availability.not_found",
            ct);
}

public sealed record CreateProviderAvailabilityRequestModel(
    Guid ProviderAvailabilityId,
    Guid ProviderId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);
public sealed record UpdateProviderAvailabilityWindowRequestModel(
    Guid ProviderAvailabilityId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);
public sealed record ProviderAvailabilityIdRequestModel(Guid ProviderAvailabilityId);
