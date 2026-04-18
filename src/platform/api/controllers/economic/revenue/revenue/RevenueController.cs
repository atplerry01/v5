using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Revenue.Revenue;

[Authorize]
[ApiController]
[Route("api/economic")]
[ApiExplorerSettings(GroupName = "economic.revenue.revenue")]
public sealed class RevenueController : ControllerBase
{
    private static readonly DomainRoute RevenueRoute = new("economic", "revenue", "revenue");

    private readonly IWorkflowDispatcher _workflowDispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public RevenueController(
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
            p.ContractId,
            p.SpvId,
            p.VaultAccountId,
            p.Amount,
            p.Currency,
            p.SourceRef);

        var result = await _workflowDispatcher.StartWorkflowAsync(
            RevenueProcessingWorkflowNames.Process, intent, RevenueRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("revenue_processing_started"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.revenue.process_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record ProcessRevenueRequestModel(
    Guid ContractId,
    string SpvId,
    Guid VaultAccountId,
    decimal Amount,
    string Currency,
    string SourceRef);
