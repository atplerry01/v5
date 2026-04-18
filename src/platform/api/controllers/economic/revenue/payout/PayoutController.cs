using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Revenue.Payout;

[Authorize]
[ApiController]
[Route("api/economic")]
[ApiExplorerSettings(GroupName = "economic.revenue.payout")]
public sealed class PayoutController : ControllerBase
{
    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    private readonly IWorkflowDispatcher _workflowDispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public PayoutController(
        IWorkflowDispatcher workflowDispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _workflowDispatcher = workflowDispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
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
            new ParticipantPayoutEntry(
                s.ParticipantId, s.ParticipantVaultId, s.Amount, s.Percentage));

        var intent = new PayoutExecutionIntent(
            payoutId,
            p.DistributionId,
            p.ContractId,
            p.SpvId,
            p.SpvVaultId,
            shares);

        var result = await _workflowDispatcher.StartWorkflowAsync(
            PayoutExecutionWorkflowNames.Execute, intent, PayoutRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("payout_execution_started"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.payout.execute_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record ExecutePayoutRequestModel(
    Guid DistributionId,
    Guid ContractId,
    string SpvId,
    Guid SpvVaultId,
    IReadOnlyList<ParticipantPayoutEntryModel> Shares);

public sealed record ParticipantPayoutEntryModel(
    string ParticipantId,
    Guid ParticipantVaultId,
    decimal Amount,
    decimal Percentage);
