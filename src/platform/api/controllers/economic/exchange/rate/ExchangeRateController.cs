using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Exchange.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Exchange.Rate;

[Authorize]
[ApiController]
[Route("api/exchange/rate")]
[ApiExplorerSettings(GroupName = "economic.exchange.rate")]
public sealed class ExchangeRateController : ExchangeControllerBase
{
    private static readonly DomainRoute RateRoute = new("economic", "exchange", "rate");

    public ExchangeRateController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("define")]
    public Task<IActionResult> Define([FromBody] ApiRequest<DefineExchangeRateRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        if (p.RateValue <= 0m)
            return Task.FromResult<IActionResult>(BadRequest(
                ApiResponse.Fail("economic.exchange.rate.invalid_rate_value", "RateValue must be positive.", Clock.UtcNow)));
        if (p.Version <= 0)
            return Task.FromResult<IActionResult>(BadRequest(
                ApiResponse.Fail("economic.exchange.rate.invalid_version", "Version must be positive.", Clock.UtcNow)));

        var rateId = IdGenerator.Generate(
            $"economic:exchange:rate:{p.BaseCurrency}:{p.QuoteCurrency}:{p.Version}");
        var cmd = new DefineExchangeRateCommand(
            rateId, p.BaseCurrency, p.QuoteCurrency, p.RateValue, p.EffectiveAt, p.Version);
        return Dispatch(cmd, RateRoute, "exchange_rate_defined", "economic.exchange.rate.define_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ActivateExchangeRateRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ActivateExchangeRateCommand(p.RateId, Clock.UtcNow);
        return Dispatch(cmd, RateRoute, "exchange_rate_activated", "economic.exchange.rate.activate_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<ExpireExchangeRateRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ExpireExchangeRateCommand(p.RateId, Clock.UtcNow);
        return Dispatch(cmd, RateRoute, "exchange_rate_expired", "economic.exchange.rate.expire_failed", ct);
    }

    [HttpGet("{rateId:guid}")]
    public Task<IActionResult> GetById(Guid rateId, CancellationToken ct) =>
        LoadReadModel<ExchangeRateReadModel>(
            rateId,
            "projection_economic_exchange_rate",
            "exchange_rate_read_model",
            "economic.exchange.rate.not_found",
            ct);

    [HttpGet("pair")]
    public Task<IActionResult> GetByPair(
        [FromQuery(Name = "base")] string baseCurrency,
        [FromQuery(Name = "quote")] string quoteCurrency,
        [FromQuery(Name = "asOf")] DateTimeOffset? asOf,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(quoteCurrency))
            return Task.FromResult<IActionResult>(BadRequest(
                ApiResponse.Fail("economic.exchange.rate.pair_missing", "'base' and 'quote' query parameters are required.", Clock.UtcNow)));

        // asOf is advisory for the canonical functional index (effectiveAt);
        // request-time filtering on the timestamp is left to the caller.
        _ = asOf;

        return LoadReadModelsByPair<ExchangeRateReadModel>(
            baseCurrency,
            quoteCurrency,
            "projection_economic_exchange_rate",
            "exchange_rate_read_model",
            ct,
            orderBy: "(state->>'effectiveAt')::timestamptz DESC");
    }
}

public sealed record DefineExchangeRateRequestModel(
    string BaseCurrency,
    string QuoteCurrency,
    decimal RateValue,
    DateTimeOffset EffectiveAt,
    int Version);

public sealed record ActivateExchangeRateRequestModel(Guid RateId);

public sealed record ExpireExchangeRateRequestModel(Guid RateId);
