using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Provider.ProviderGovernance.ProviderAgreement;

[Authorize]
[ApiController]
[Route("api/provider-governance/provider-agreement")]
[ApiExplorerSettings(GroupName = "business.provider.provider-governance.provider-agreement")]
public sealed class ProviderAgreementController : BusinessControllerBase
{
    private static readonly DomainRoute ProviderAgreementRoute = new("business", "provider", "provider-agreement");

    public ProviderAgreementController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateProviderAgreementRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateProviderAgreementCommand(
            p.ProviderAgreementId,
            p.ProviderId,
            p.ContractId,
            p.EffectiveStartsAt,
            p.EffectiveEndsAt);
        return Dispatch(cmd, ProviderAgreementRoute, "provider_agreement_created", "business.provider.provider-agreement.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateProviderAgreementRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ActivateProviderAgreementCommand(p.ProviderAgreementId, p.EffectiveStartsAt, p.EffectiveEndsAt);
        return Dispatch(cmd, ProviderAgreementRoute, "provider_agreement_activated", "business.provider.provider-agreement.activate_failed", ct);
    }

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<ProviderAgreementIdRequestModel> request, CancellationToken ct)
    {
        // SuspendedAt is read from IClock (host-bound wall clock in production;
        // TestClock in tests). Caller supplies only the stable aggregate id so
        // the dispatch seed derives from deterministic command coordinates.
        var cmd = new SuspendProviderAgreementCommand(request.Data.ProviderAgreementId, Clock.UtcNow);
        return Dispatch(cmd, ProviderAgreementRoute, "provider_agreement_suspended", "business.provider.provider-agreement.suspend_failed", ct);
    }

    [HttpPost("terminate")]
    public Task<IActionResult> Terminate([FromBody] ApiRequest<ProviderAgreementIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new TerminateProviderAgreementCommand(request.Data.ProviderAgreementId, Clock.UtcNow);
        return Dispatch(cmd, ProviderAgreementRoute, "provider_agreement_terminated", "business.provider.provider-agreement.terminate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProviderAgreement(Guid id, CancellationToken ct) =>
        LoadReadModel<ProviderAgreementReadModel>(
            id,
            "projection_business_provider_provider_agreement",
            "provider_agreement_read_model",
            "business.provider.provider-agreement.not_found",
            ct);
}

public sealed record CreateProviderAgreementRequestModel(
    Guid ProviderAgreementId,
    Guid ProviderId,
    Guid? ContractId,
    DateTimeOffset? EffectiveStartsAt,
    DateTimeOffset? EffectiveEndsAt);

public sealed record ActivateProviderAgreementRequestModel(
    Guid ProviderAgreementId,
    DateTimeOffset EffectiveStartsAt,
    DateTimeOffset? EffectiveEndsAt);

public sealed record ProviderAgreementIdRequestModel(Guid ProviderAgreementId);
