using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Observability.SystemMetric;

[Authorize]
[ApiController]
[Route("api/control/system-metric")]
[ApiExplorerSettings(GroupName = "control.observability.system-metric")]
public sealed class SystemMetricController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "observability", "system-metric");

    public SystemMetricController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineSystemMetricRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineSystemMetricCommand(p.MetricId, p.Name, p.Type, p.Unit, p.LabelNames);
        return Dispatch(cmd, Route, "system_metric_defined", "control.observability.system-metric.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<SystemMetricIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateSystemMetricCommand(request.Data.MetricId);
        return Dispatch(cmd, Route, "system_metric_deprecated", "control.observability.system-metric.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<SystemMetricReadModel>(
            id,
            "projection_control_observability_system_metric",
            "system_metric_read_model",
            "control.observability.system-metric.not_found",
            ct);
}

public sealed record DefineSystemMetricRequestModel(
    Guid MetricId,
    string Name,
    string Type,
    string Unit,
    IReadOnlyList<string> LabelNames);

public sealed record SystemMetricIdRequestModel(Guid MetricId);
