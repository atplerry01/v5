using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Configuration.ConfigurationState;

[Authorize]
[ApiController]
[Route("api/control/configuration-state")]
[ApiExplorerSettings(GroupName = "control.configuration.configuration-state")]
public sealed class ConfigurationStateController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "configuration", "configuration-state");

    public ConfigurationStateController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("set")]
    public Task<IActionResult> Set([FromBody] ApiRequest<SetConfigurationStateRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SetConfigurationStateCommand(p.StateId, p.DefinitionId, p.Value, p.Version);
        return Dispatch(cmd, Route, "configuration_state_set", "control.configuration.configuration-state.set_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<StateIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeConfigurationStateCommand(request.Data.StateId);
        return Dispatch(cmd, Route, "configuration_state_revoked", "control.configuration.configuration-state.revoke_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ConfigurationStateReadModel>(
            id,
            "projection_control_configuration_configuration_state",
            "configuration_state_read_model",
            "control.configuration.configuration-state.not_found",
            ct);
}

public sealed record SetConfigurationStateRequestModel(Guid StateId, string DefinitionId, string Value, int Version);
public sealed record StateIdRequestModel(Guid StateId);
