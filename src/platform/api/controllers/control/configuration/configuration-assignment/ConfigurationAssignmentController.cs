using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Configuration.ConfigurationAssignment;

[Authorize]
[ApiController]
[Route("api/control/configuration-assignment")]
[ApiExplorerSettings(GroupName = "control.configuration.configuration-assignment")]
public sealed class ConfigurationAssignmentController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "configuration", "configuration-assignment");

    public ConfigurationAssignmentController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("assign")]
    public Task<IActionResult> Assign([FromBody] ApiRequest<AssignConfigurationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AssignConfigurationCommand(p.AssignmentId, p.DefinitionId, p.ScopeId, p.Value);
        return Dispatch(cmd, Route, "configuration_assigned", "control.configuration.configuration-assignment.assign_failed", ct);
    }

    [HttpPost("revoke")]
    public Task<IActionResult> Revoke([FromBody] ApiRequest<AssignmentIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RevokeConfigurationAssignmentCommand(request.Data.AssignmentId);
        return Dispatch(cmd, Route, "configuration_assignment_revoked", "control.configuration.configuration-assignment.revoke_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ConfigurationAssignmentReadModel>(
            id,
            "projection_control_configuration_configuration_assignment",
            "configuration_assignment_read_model",
            "control.configuration.configuration-assignment.not_found",
            ct);
}

public sealed record AssignConfigurationRequestModel(Guid AssignmentId, string DefinitionId, string ScopeId, string Value);
public sealed record AssignmentIdRequestModel(Guid AssignmentId);
