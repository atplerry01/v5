using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.Configuration.ConfigurationScope;

[Authorize]
[ApiController]
[Route("api/control/configuration-scope")]
[ApiExplorerSettings(GroupName = "control.configuration.configuration-scope")]
public sealed class ConfigurationScopeController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "configuration", "configuration-scope");

    public ConfigurationScopeController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("declare")]
    public Task<IActionResult> Declare([FromBody] ApiRequest<DeclareConfigurationScopeRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DeclareConfigurationScopeCommand(p.ScopeId, p.DefinitionId, p.Classification, p.Context);
        return Dispatch(cmd, Route, "configuration_scope_declared", "control.configuration.configuration-scope.declare_failed", ct);
    }

    [HttpPost("remove")]
    public Task<IActionResult> Remove([FromBody] ApiRequest<ScopeIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RemoveConfigurationScopeCommand(request.Data.ScopeId);
        return Dispatch(cmd, Route, "configuration_scope_removed", "control.configuration.configuration-scope.remove_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<ConfigurationScopeReadModel>(
            id,
            "projection_control_configuration_configuration_scope",
            "configuration_scope_read_model",
            "control.configuration.configuration-scope.not_found",
            ct);
}

public sealed record DeclareConfigurationScopeRequestModel(Guid ScopeId, string DefinitionId, string Classification, string? Context = null);
public sealed record ScopeIdRequestModel(Guid ScopeId);
