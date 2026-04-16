using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Exchange.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Exchange.Fx;

[Authorize]
[ApiController]
[Route("api/exchange/fx")]
[ApiExplorerSettings(GroupName = "economic.exchange.fx")]
public sealed class FxController : ExchangeControllerBase
{
    private static readonly DomainRoute FxRoute = new("economic", "exchange", "fx");

    public FxController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterFxPairRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var fxId = IdGenerator.Generate($"economic:exchange:fx:{p.BaseCurrency}:{p.QuoteCurrency}");
        var cmd = new RegisterFxPairCommand(fxId, p.BaseCurrency, p.QuoteCurrency);
        return Dispatch(cmd, FxRoute, "fx_pair_registered", "economic.exchange.fx.register_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateFxPairRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ActivateFxPairCommand(p.FxId, Clock.UtcNow);
        return Dispatch(cmd, FxRoute, "fx_pair_activated", "economic.exchange.fx.activate_failed", ct);
    }

    [HttpPost("deactivate")]
    public Task<IActionResult> Deactivate([FromBody] ApiRequest<DeactivateFxPairRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DeactivateFxPairCommand(p.FxId, Clock.UtcNow);
        return Dispatch(cmd, FxRoute, "fx_pair_deactivated", "economic.exchange.fx.deactivate_failed", ct);
    }

    [HttpGet("{fxId:guid}")]
    public Task<IActionResult> GetById(Guid fxId, CancellationToken ct) =>
        LoadReadModel<FxReadModel>(
            fxId,
            "projection_economic_exchange_fx",
            "fx_read_model",
            "economic.exchange.fx.not_found",
            ct);

    [HttpGet("pair")]
    public Task<IActionResult> GetByPair(
        [FromQuery(Name = "base")] string baseCurrency,
        [FromQuery(Name = "quote")] string quoteCurrency,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(quoteCurrency))
            return Task.FromResult<IActionResult>(BadRequest(
                ApiResponse.Fail("economic.exchange.fx.pair_missing", "'base' and 'quote' query parameters are required.", Clock.UtcNow)));

        return LoadReadModelsByPair<FxReadModel>(
            baseCurrency,
            quoteCurrency,
            "projection_economic_exchange_fx",
            "fx_read_model",
            ct);
    }
}

public sealed record RegisterFxPairRequestModel(string BaseCurrency, string QuoteCurrency);
public sealed record ActivateFxPairRequestModel(Guid FxId);
public sealed record DeactivateFxPairRequestModel(Guid FxId);
