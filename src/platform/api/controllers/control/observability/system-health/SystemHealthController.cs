using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Observability.SystemHealth;

[Authorize]
[ApiController]
[Route("api/control/system-health")]
[ApiExplorerSettings(GroupName = "control.observability.system-health")]
public sealed class SystemHealthController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "observability", "system-health");

    public SystemHealthController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("evaluate")]
    public Task<IActionResult> Evaluate([FromBody] ApiRequest<EvaluateSystemHealthRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new EvaluateSystemHealthCommand(p.HealthId, p.ComponentName, p.Status, p.EvaluatedAt);
        return Dispatch(cmd, Route, "system_health_evaluated", "control.observability.system-health.evaluate_failed", ct);
    }

    [HttpPost("record-degradation")]
    public Task<IActionResult> RecordDegradation([FromBody] ApiRequest<RecordSystemHealthDegradationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RecordSystemHealthDegradationCommand(p.HealthId, p.NewStatus, p.Reason, p.OccurredAt);
        return Dispatch(cmd, Route, "system_health_degraded", "control.observability.system-health.record_degradation_failed", ct);
    }

    [HttpPost("restore")]
    public Task<IActionResult> Restore([FromBody] ApiRequest<RestoreSystemHealthRequestModel> request, CancellationToken ct)
    {
        var cmd = new RestoreSystemHealthCommand(request.Data.HealthId, request.Data.RestoredAt);
        return Dispatch(cmd, Route, "system_health_restored", "control.observability.system-health.restore_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<SystemHealthReadModel>(
            id,
            "projection_control_observability_system_health",
            "system_health_read_model",
            "control.observability.system-health.not_found",
            ct);
}

public sealed record EvaluateSystemHealthRequestModel(
    Guid HealthId,
    string ComponentName,
    string Status,
    DateTimeOffset EvaluatedAt);

public sealed record RecordSystemHealthDegradationRequestModel(
    Guid HealthId,
    string NewStatus,
    string Reason,
    DateTimeOffset OccurredAt);

public sealed record RestoreSystemHealthRequestModel(Guid HealthId, DateTimeOffset RestoredAt);
