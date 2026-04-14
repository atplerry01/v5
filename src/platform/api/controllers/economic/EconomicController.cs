using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic;

[Authorize]
[ApiController]
[Route("api/economic")]
[ApiExplorerSettings(GroupName = "economic.revenue")]
public sealed class EconomicController : ControllerBase
{
    private static readonly DomainRoute RevenueRoute = new("economic", "revenue", "revenue");
    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");
    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    private readonly IWorkflowDispatcher _workflowDispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public EconomicController(
        IWorkflowDispatcher workflowDispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _workflowDispatcher = workflowDispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/economic/revenue/process ──────────────────────

    [HttpPost("revenue/process")]
    public async Task<IActionResult> ProcessRevenue(
        [FromBody] ApiRequest<ProcessRevenueRequestModel> request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken; // workflow dispatcher does not currently accept a token
        var p = request.Data;
        var revenueId = _idGenerator.Generate($"economic:revenue:{p.SpvId}:{p.SourceRef}:{p.Amount}:{p.Currency}");

        var intent = new RevenueProcessingIntent(
            revenueId,
            p.SpvId,
            p.VaultAccountId,
            p.Amount,
            p.Currency,
            p.SourceRef);

        var result = await _workflowDispatcher.StartWorkflowAsync(
            RevenueProcessingWorkflowNames.Process, intent, RevenueRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("revenue_processing_started"), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.revenue.process_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
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
            p.SpvId,
            p.TotalAmount,
            allocations);

        var result = await _workflowDispatcher.StartWorkflowAsync(
            DistributionWorkflowNames.Create, intent, DistributionRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("distribution_creation_started"), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.distribution.create_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    // ── POST /api/economic/payout/execute ───────────────────────

    [HttpPost("payout/execute")]
    public async Task<IActionResult> ExecutePayout(
        [FromBody] ApiRequest<ExecutePayoutRequestModel> request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var p = request.Data;
        var payoutId = _idGenerator.Generate($"economic:payout:{p.DistributionId}:{p.SpvId}");

        var shares = new List<ParticipantPayoutEntryModel>(p.Shares).ConvertAll(s =>
            new Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow.ParticipantPayoutEntry(
                s.ParticipantId, s.ParticipantVaultId, s.Amount, s.Percentage));

        var intent = new PayoutExecutionIntent(
            payoutId,
            p.DistributionId,
            p.SpvId,
            p.SpvVaultId,
            shares);

        var result = await _workflowDispatcher.StartWorkflowAsync(
            PayoutExecutionWorkflowNames.Execute, intent, PayoutRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("payout_execution_started"), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.payout.execute_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record ProcessRevenueRequestModel(
    string SpvId,
    Guid VaultAccountId,
    decimal Amount,
    string Currency,
    string SourceRef);

public sealed record CreateDistributionRequestModel(
    string SpvId,
    decimal TotalAmount,
    IReadOnlyList<AllocationRequestModel> Allocations);

public sealed record AllocationRequestModel(
    string ParticipantId,
    decimal OwnershipPercentage);

public sealed record ExecutePayoutRequestModel(
    Guid DistributionId,
    string SpvId,
    Guid SpvVaultId,
    IReadOnlyList<ParticipantPayoutEntryModel> Shares);

public sealed record ParticipantPayoutEntryModel(
    string ParticipantId,
    Guid ParticipantVaultId,
    decimal Amount,
    decimal Percentage);
