using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Offering.CommercialShape.Configuration;

[Authorize]
[ApiController]
[Route("api/commercial-shape/configuration")]
[ApiExplorerSettings(GroupName = "business.offering.commercial-shape.configuration")]
public sealed class ConfigurationController : BusinessControllerBase
{
    // DomainRoute is the three-tuple (classification, context, domain) per CLAUDE.md $1a
    // even when the physical nesting is four-level (classification/context/domain-group/domain).
    private static readonly DomainRoute ConfigurationRoute = new("business", "offering", "configuration");

    public ConfigurationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateConfigurationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // ConfigurationId MUST be supplied by the caller per DET-SEED-DERIVATION-01 —
        // no clock/Random entropy on the API hot path.
        var cmd = new CreateConfigurationCommand(p.ConfigurationId, p.Name);
        return Dispatch(cmd, ConfigurationRoute, "configuration_created", "business.offering.configuration.create_failed", ct);
    }

    [HttpPost("set-option")]
    public Task<IActionResult> SetOption([FromBody] ApiRequest<SetConfigurationOptionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new SetConfigurationOptionCommand(p.ConfigurationId, p.Key, p.Value);
        return Dispatch(cmd, ConfigurationRoute, "configuration_option_set", "business.offering.configuration.set_option_failed", ct);
    }

    [HttpPost("remove-option")]
    public Task<IActionResult> RemoveOption([FromBody] ApiRequest<RemoveConfigurationOptionRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RemoveConfigurationOptionCommand(p.ConfigurationId, p.Key);
        return Dispatch(cmd, ConfigurationRoute, "configuration_option_removed", "business.offering.configuration.remove_option_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ConfigurationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateConfigurationCommand(request.Data.ConfigurationId);
        return Dispatch(cmd, ConfigurationRoute, "configuration_activated", "business.offering.configuration.activate_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<ConfigurationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveConfigurationCommand(request.Data.ConfigurationId);
        return Dispatch(cmd, ConfigurationRoute, "configuration_archived", "business.offering.configuration.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetConfiguration(Guid id, CancellationToken ct) =>
        LoadReadModel<ConfigurationReadModel>(
            id,
            "projection_business_offering_configuration",
            "configuration_read_model",
            "business.offering.configuration.not_found",
            ct);
}

public sealed record CreateConfigurationRequestModel(Guid ConfigurationId, string Name);
public sealed record SetConfigurationOptionRequestModel(Guid ConfigurationId, string Key, string Value);
public sealed record RemoveConfigurationOptionRequestModel(Guid ConfigurationId, string Key);
public sealed record ConfigurationIdRequestModel(Guid ConfigurationId);
