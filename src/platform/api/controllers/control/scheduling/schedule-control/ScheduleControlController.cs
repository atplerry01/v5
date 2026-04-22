using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Scheduling.ScheduleControl;

[Authorize]
[ApiController]
[Route("api/control/schedule-control")]
[ApiExplorerSettings(GroupName = "control.scheduling.schedule-control")]
public sealed class ScheduleControlController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "scheduling", "schedule-control");

    public ScheduleControlController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineScheduleControlRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineScheduleControlCommand(p.ScheduleId, p.JobDefinitionId, p.TriggerExpression);
        return Dispatch(cmd, Route, "schedule_control_defined", "control.scheduling.schedule-control.define_failed", ct);
    }

    [HttpPost("suspend")]
    public Task<IActionResult> Suspend([FromBody] ApiRequest<ScheduleControlIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SuspendScheduleControlCommand(request.Data.ScheduleId);
        return Dispatch(cmd, Route, "schedule_control_suspended", "control.scheduling.schedule-control.suspend_failed", ct);
    }

    [HttpPost("resume")]
    public Task<IActionResult> Resume([FromBody] ApiRequest<ScheduleControlIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ResumeScheduleControlCommand(request.Data.ScheduleId);
        return Dispatch(cmd, Route, "schedule_control_resumed", "control.scheduling.schedule-control.resume_failed", ct);
    }

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<ScheduleControlIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RetireScheduleControlCommand(request.Data.ScheduleId);
        return Dispatch(cmd, Route, "schedule_control_retired", "control.scheduling.schedule-control.retire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ScheduleControlReadModel>(
            id,
            "projection_control_scheduling_schedule_control",
            "schedule_control_read_model",
            "control.scheduling.schedule-control.not_found",
            ct);
}

public sealed record DefineScheduleControlRequestModel(
    Guid ScheduleId,
    string JobDefinitionId,
    string TriggerExpression);

public sealed record ScheduleControlIdRequestModel(Guid ScheduleId);
