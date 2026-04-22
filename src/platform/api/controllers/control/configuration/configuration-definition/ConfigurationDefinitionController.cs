using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Configuration.ConfigurationDefinition;

[Authorize]
[ApiController]
[Route("api/control/configuration-definition")]
[ApiExplorerSettings(GroupName = "control.configuration.configuration-definition")]
public sealed class ConfigurationDefinitionController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "configuration", "configuration-definition");

    public ConfigurationDefinitionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineConfigurationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DefineConfigurationCommand(p.DefinitionId, p.Name, p.ValueType, p.Description, p.DefaultValue);
        return Dispatch(cmd, Route, "configuration_defined", "control.configuration.configuration-definition.define_failed", ct);
    }

    [HttpPost("deprecate")]
    public Task<IActionResult> Deprecate([FromBody] ApiRequest<DefinitionIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeprecateConfigurationDefinitionCommand(request.Data.DefinitionId);
        return Dispatch(cmd, Route, "configuration_definition_deprecated", "control.configuration.configuration-definition.deprecate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ConfigurationDefinitionReadModel>(
            id,
            "projection_control_configuration_configuration_definition",
            "configuration_definition_read_model",
            "control.configuration.configuration-definition.not_found",
            ct);
}

public sealed record DefineConfigurationRequestModel(Guid DefinitionId, string Name, string ValueType, string Description, string? DefaultValue = null);
public sealed record DefinitionIdRequestModel(Guid DefinitionId);
