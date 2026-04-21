using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Provider.ProviderScope.ProviderCoverage;

[Authorize]
[ApiController]
[Route("api/provider-scope/provider-coverage")]
[ApiExplorerSettings(GroupName = "business.provider.provider-scope.provider-coverage")]
public sealed class ProviderCoverageController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ProviderCoverageRoute = new("business", "provider", "provider-coverage");

    public ProviderCoverageController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateProviderCoverageRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ProviderCoverageId MUST be supplied by the caller per DET-SEED-DERIVATION-01.
        var cmd = new CreateProviderCoverageCommand(p.ProviderCoverageId, p.ProviderId);
        return Dispatch(cmd, ProviderCoverageRoute, "provider_coverage_created", "business.provider.provider-coverage.create_failed", ct);
    }

    [HttpPost("add-scope")]
    public Task<IActionResult> AddScope([FromBody] ApiRequest<CoverageScopeMutationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AddCoverageScopeCommand(p.ProviderCoverageId, p.ScopeKind, p.ScopeDescriptor);
        return Dispatch(cmd, ProviderCoverageRoute, "coverage_scope_added", "business.provider.provider-coverage.add_scope_failed", ct);
    }

    [HttpPost("remove-scope")]
    public Task<IActionResult> RemoveScope([FromBody] ApiRequest<CoverageScopeMutationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveCoverageScopeCommand(p.ProviderCoverageId, p.ScopeKind, p.ScopeDescriptor);
        return Dispatch(cmd, ProviderCoverageRoute, "coverage_scope_removed", "business.provider.provider-coverage.remove_scope_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ProviderCoverageIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateProviderCoverageCommand(request.Data.ProviderCoverageId);
        return Dispatch(cmd, ProviderCoverageRoute, "provider_coverage_activated", "business.provider.provider-coverage.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ProviderCoverageIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveProviderCoverageCommand(request.Data.ProviderCoverageId);
        return Dispatch(cmd, ProviderCoverageRoute, "provider_coverage_archived", "business.provider.provider-coverage.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetProviderCoverage(Guid id, CancellationToken ct) =>
        LoadReadModel<ProviderCoverageReadModel>(
            id,
            "projection_business_provider_provider_coverage",
            "provider_coverage_read_model",
            "business.provider.provider-coverage.not_found",
            ct);
}

public sealed record CreateProviderCoverageRequestModel(Guid ProviderCoverageId, Guid ProviderId);
public sealed record CoverageScopeMutationRequestModel(Guid ProviderCoverageId, string ScopeKind, string ScopeDescriptor);
public sealed record ProviderCoverageIdRequestModel(Guid ProviderCoverageId);
