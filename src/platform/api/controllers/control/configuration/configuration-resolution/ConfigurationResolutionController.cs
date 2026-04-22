using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Configuration.ConfigurationResolution;

[Authorize]
[ApiController]
[Route("api/control/configuration-resolution")]
[ApiExplorerSettings(GroupName = "control.configuration.configuration-resolution")]
public sealed class ConfigurationResolutionController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "configuration", "configuration-resolution");

    public ConfigurationResolutionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("record")]
    public Task<IActionResult> Record([FromBody] ApiRequest<RecordConfigurationResolutionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RecordConfigurationResolutionCommand(p.ResolutionId, p.DefinitionId, p.ScopeId, p.StateId, p.ResolvedValue, p.ResolvedAt);
        return Dispatch(cmd, Route, "configuration_resolution_recorded", "control.configuration.configuration-resolution.record_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ConfigurationResolutionReadModel>(
            id,
            "projection_control_configuration_configuration_resolution",
            "configuration_resolution_read_model",
            "control.configuration.configuration-resolution.not_found",
            ct);
}

public sealed record RecordConfigurationResolutionRequestModel(
    Guid ResolutionId, string DefinitionId, string ScopeId, string StateId, string ResolvedValue, DateTimeOffset ResolvedAt);
