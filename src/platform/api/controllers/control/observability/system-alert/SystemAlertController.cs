using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Observability.SystemAlert;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Observability.SystemAlert;

[Authorize]
[ApiController]
[Route("api/control/system-alert")]
[ApiExplorerSettings(GroupName = "control.observability.system-alert")]
public sealed class SystemAlertController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "observability", "system-alert");

    public SystemAlertController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineSystemAlertRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineSystemAlertCommand(p.AlertId, p.Name, p.MetricDefinitionId, p.ConditionExpression, p.Severity);
        return Dispatch(cmd, Route, "system_alert_defined", "control.observability.system-alert.define_failed", ct);
    }

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<SystemAlertIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RetireSystemAlertCommand(request.Data.AlertId);
        return Dispatch(cmd, Route, "system_alert_retired", "control.observability.system-alert.retire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<SystemAlertReadModel>(
            id,
            "projection_control_observability_system_alert",
            "system_alert_read_model",
            "control.observability.system-alert.not_found",
            ct);
}

public sealed record DefineSystemAlertRequestModel(
    Guid AlertId,
    string Name,
    string MetricDefinitionId,
    string ConditionExpression,
    string Severity);

public sealed record SystemAlertIdRequestModel(Guid AlertId);
