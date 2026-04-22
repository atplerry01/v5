using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Scheduling.SystemJob;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Scheduling.SystemJob;

[Authorize]
[ApiController]
[Route("api/control/system-job")]
[ApiExplorerSettings(GroupName = "control.scheduling.system-job")]
public sealed class SystemJobController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "scheduling", "system-job");

    public SystemJobController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineSystemJobRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineSystemJobCommand(p.JobId, p.Name, p.Category, p.Timeout);
        return Dispatch(cmd, Route, "system_job_defined", "control.scheduling.system-job.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<SystemJobIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateSystemJobCommand(request.Data.JobId);
        return Dispatch(cmd, Route, "system_job_deprecated", "control.scheduling.system-job.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<SystemJobReadModel>(
            id,
            "projection_control_scheduling_system_job",
            "system_job_read_model",
            "control.scheduling.system-job.not_found",
            ct);
}

public sealed record DefineSystemJobRequestModel(
    Guid JobId,
    string Name,
    string Category,
    TimeSpan Timeout);

public sealed record SystemJobIdRequestModel(Guid JobId);
