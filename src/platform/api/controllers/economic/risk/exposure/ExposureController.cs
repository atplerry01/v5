using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Risk.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Risk.Exposure;

[Authorize]
[ApiController]
[Route("api/risk/exposure")]
[ApiExplorerSettings(GroupName = "economic.risk.exposure")]
public sealed class ExposureController : RiskControllerBase
{
    private static readonly DomainRoute ExposureRoute = new("economic", "risk", "exposure");

    public ExposureController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> CreateExposure([FromBody] ApiRequest<CreateExposureRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var exposureId = IdGenerator.Generate($"economic:risk:exposure:{p.SourceId}:{p.ExposureType}:{p.Currency}");
        var cmd = new CreateRiskExposureCommand(
            exposureId, p.SourceId, p.ExposureType, p.InitialExposure, p.Currency, Clock.UtcNow);
        return Dispatch(cmd, ExposureRoute, "risk_exposure_created", "economic.risk.exposure.create_failed", ct);
    }

    [HttpPost("increase")]
    public Task<IActionResult> IncreaseExposure([FromBody] ApiRequest<IncreaseExposureRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new IncreaseRiskExposureCommand(p.ExposureId, p.Amount);
        return Dispatch(cmd, ExposureRoute, "risk_exposure_increased", "economic.risk.exposure.increase_failed", ct);
    }

    [HttpPost("reduce")]
    public Task<IActionResult> ReduceExposure([FromBody] ApiRequest<ReduceExposureRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReduceRiskExposureCommand(p.ExposureId, p.Amount);
        return Dispatch(cmd, ExposureRoute, "risk_exposure_reduced", "economic.risk.exposure.reduce_failed", ct);
    }

    [HttpPost("close")]
    public Task<IActionResult> CloseExposure([FromBody] ApiRequest<CloseExposureRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CloseRiskExposureCommand(p.ExposureId);
        return Dispatch(cmd, ExposureRoute, "risk_exposure_closed", "economic.risk.exposure.close_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetExposure(Guid id, CancellationToken ct) =>
        LoadReadModel<RiskExposureReadModel>(
            id,
            "projection_economic_risk_exposure",
            "risk_exposure_read_model",
            "economic.risk.exposure.not_found",
            ct);
}

public sealed record CreateExposureRequestModel(
    Guid SourceId,
    int ExposureType,
    decimal InitialExposure,
    string Currency);

public sealed record IncreaseExposureRequestModel(Guid ExposureId, decimal Amount);
public sealed record ReduceExposureRequestModel(Guid ExposureId, decimal Amount);
public sealed record CloseExposureRequestModel(Guid ExposureId);
