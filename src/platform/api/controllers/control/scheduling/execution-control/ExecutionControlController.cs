using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Scheduling.ExecutionControl;

[Authorize]
[ApiController]
[Route("api/control/execution-control")]
[ApiExplorerSettings(GroupName = "control.scheduling.execution-control")]
public sealed class ExecutionControlController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "scheduling", "execution-control");

    public ExecutionControlController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("issue")]
    public Task<IActionResult> Issue([FromBody] ApiRequest<IssueExecutionControlRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new IssueExecutionControlCommand(p.ControlId, p.JobInstanceId, p.Signal, p.ActorId, p.IssuedAt);
        return Dispatch(cmd, Route, "execution_control_issued", "control.scheduling.execution-control.issue_failed", ct);
    }

    [HttpPost("record-outcome")]
    public Task<IActionResult> RecordOutcome([FromBody] ApiRequest<RecordExecutionControlOutcomeRequestModel> request, CancellationToken ct)
    {
        var cmd = new RecordExecutionControlOutcomeCommand(request.Data.ControlId, request.Data.Outcome);
        return Dispatch(cmd, Route, "execution_control_outcome_recorded", "control.scheduling.execution-control.record_outcome_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ExecutionControlReadModel>(
            id,
            "projection_control_scheduling_execution_control",
            "execution_control_read_model",
            "control.scheduling.execution-control.not_found",
            ct);
}

public sealed record IssueExecutionControlRequestModel(
    Guid ControlId,
    string JobInstanceId,
    string Signal,
    string ActorId,
    DateTimeOffset IssuedAt);

public sealed record RecordExecutionControlOutcomeRequestModel(Guid ControlId, string Outcome);
