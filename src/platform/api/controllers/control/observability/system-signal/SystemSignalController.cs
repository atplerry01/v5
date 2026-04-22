using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Observability.SystemSignal;

[Authorize]
[ApiController]
[Route("api/control/system-signal")]
[ApiExplorerSettings(GroupName = "control.observability.system-signal")]
public sealed class SystemSignalController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "observability", "system-signal");

    public SystemSignalController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineSystemSignalRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineSystemSignalCommand(p.SignalId, p.Name, p.Kind, p.Source);
        return Dispatch(cmd, Route, "system_signal_defined", "control.observability.system-signal.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<SystemSignalIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateSystemSignalCommand(request.Data.SignalId);
        return Dispatch(cmd, Route, "system_signal_deprecated", "control.observability.system-signal.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<SystemSignalReadModel>(
            id,
            "projection_control_observability_system_signal",
            "system_signal_read_model",
            "control.observability.system-signal.not_found",
            ct);
}

public sealed record DefineSystemSignalRequestModel(
    Guid SignalId,
    string Name,
    string Kind,
    string Source);

public sealed record SystemSignalIdRequestModel(Guid SignalId);
