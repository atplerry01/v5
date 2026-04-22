using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemReconciliation.ConsistencyCheck;

[Authorize]
[ApiController]
[Route("api/control/consistency-check")]
[ApiExplorerSettings(GroupName = "control.system-reconciliation.consistency-check")]
public sealed class ConsistencyCheckController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-reconciliation", "consistency-check");

    public ConsistencyCheckController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("initiate")]
    public Task<IActionResult> Initiate([FromBody] ApiRequest<InitiateConsistencyCheckRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new InitiateConsistencyCheckCommand(p.CheckId, p.ScopeTarget, p.InitiatedAt);
        return Dispatch(cmd, Route, "consistency_check_initiated", "control.system-reconciliation.consistency-check.initiate_failed", ct);
    }

    [HttpPost("complete")]
    public Task<IActionResult> Complete([FromBody] ApiRequest<CompleteConsistencyCheckRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteConsistencyCheckCommand(p.CheckId, p.HasDiscrepancies, p.CompletedAt);
        return Dispatch(cmd, Route, "consistency_check_completed", "control.system-reconciliation.consistency-check.complete_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ConsistencyCheckReadModel>(
            id,
            "projection_control_system_reconciliation_consistency_check",
            "consistency_check_read_model",
            "control.system-reconciliation.consistency-check.not_found",
            ct);
}

public sealed record InitiateConsistencyCheckRequestModel(
    Guid CheckId,
    string ScopeTarget,
    DateTimeOffset InitiatedAt);

public sealed record CompleteConsistencyCheckRequestModel(
    Guid CheckId,
    bool HasDiscrepancies,
    DateTimeOffset CompletedAt);
