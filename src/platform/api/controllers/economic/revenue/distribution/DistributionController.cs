using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Revenue.Distribution;

[Authorize]
[ApiController]
[Route("api/economic")]
[ApiExplorerSettings(GroupName = "economic.revenue.distribution")]
public sealed class DistributionController : ControllerBase
{
    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");

    private readonly IWorkflowDispatcher _workflowDispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public DistributionController(
        IWorkflowDispatcher workflowDispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _workflowDispatcher = workflowDispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/economic/distribution/create ──────────────────

    [HttpPost("distribution/create")]
    public async Task<IActionResult> CreateDistribution(
        [FromBody] ApiRequest<CreateDistributionRequestModel> request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var p = request.Data;
        var distributionId = _idGenerator.Generate($"economic:distribution:{p.SpvId}:{p.TotalAmount}");

        var allocations = new List<DistributionAllocation>(p.Allocations.Count);
        foreach (var a in p.Allocations)
            allocations.Add(new DistributionAllocation(a.ParticipantId, a.OwnershipPercentage));

        var intent = new DistributionCreationIntent(
            distributionId,
            p.ContractId,
            p.SpvId,
            p.TotalAmount,
            allocations);

        var result = await _workflowDispatcher.StartWorkflowAsync(
            DistributionWorkflowNames.Create, intent, DistributionRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("distribution_creation_started"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.distribution.create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record CreateDistributionRequestModel(
    Guid ContractId,
    string SpvId,
    decimal TotalAmount,
    IReadOnlyList<AllocationRequestModel> Allocations);

public sealed record AllocationRequestModel(
    string ParticipantId,
    decimal OwnershipPercentage);
