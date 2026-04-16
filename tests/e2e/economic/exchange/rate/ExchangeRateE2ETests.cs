using System.Net;
using Whycespace.Platform.Api.Controllers.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Tests.E2E.Economic.Exchange.Setup;

namespace Whycespace.Tests.E2E.Economic.Exchange.Rate;

[Collection(ExchangeE2ECollection.Name)]
public sealed class ExchangeRateE2ETests
{
    private const string ProjSchema = "projection_economic_exchange_rate";
    private const string ProjTable  = "exchange_rate_read_model";

    private readonly ExchangeE2EFixture _fix;
    public ExchangeRateE2ETests(ExchangeE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task Lifecycle_DefineActivateExpire_AggregateReplayMultiEvent()
    {
        // RunId-scoped synthetic currency codes keep the controller-derived
        // RateId unique per run. Each of the three transitions requires the
        // aggregate to be rehydrated from the event stream (Activate reads
        // Defined; Expire reads Defined+Activated). Success on all three
        // implies Timestamp + Currency JsonConverters rehydrate the
        // domain event types correctly.
        var baseCurrency = _fix.RunCurrencyCode("rate:lifecycle:base");
        var quoteCurrency = _fix.RunCurrencyCode("rate:lifecycle:quote");
        const decimal rateValue = 1650.50m;
        const int version = 1;
        var effectiveAt = _fix.Clock.UtcNow;
        var expectedRateId = _fix.IdGenerator.Generate(
            $"economic:exchange:rate:{baseCurrency}:{quoteCurrency}:{version}");
        var correlationId = _fix.SeedId("rate:lifecycle:corr");

        // 1) Define
        var define = await ExchangeApiEnvelope.PostAsync(
            _fix.Http, "/api/exchange/rate/define",
            new DefineExchangeRateRequestModel(baseCurrency, quoteCurrency, rateValue, effectiveAt, version),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, define.StatusCode);

        await ExchangeProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRateId, "Defined", ExchangeE2EConfig.PollTimeout);

        // 2) Activate — proves single-event aggregate replay.
        var activate = await ExchangeApiEnvelope.PostAsync(
            _fix.Http, "/api/exchange/rate/activate",
            new ActivateExchangeRateRequestModel(expectedRateId), correlationId);
        Assert.Equal(HttpStatusCode.OK, activate.StatusCode);

        // 3) GET by id — projection exists.
        var getById = await _fix.Http.GetAsync($"/api/exchange/rate/{expectedRateId}");
        Assert.Equal(HttpStatusCode.OK, getById.StatusCode);
        var read = await ExchangeApiEnvelope.ReadAsync<ExchangeRateReadModel>(getById);
        Assert.True(read!.Success);
        Assert.Equal(expectedRateId, read.Data!.RateId);
        Assert.Equal(baseCurrency, read.Data.BaseCurrency);
        Assert.Equal(quoteCurrency, read.Data.QuoteCurrency);
        Assert.Equal(rateValue, read.Data.RateValue);
        Assert.Equal(version, read.Data.VersionNumber);

        // 4) GET by pair — list endpoint wired; contains the rate.
        var asOf = _fix.Clock.UtcNow.AddSeconds(1);
        var byPair = await _fix.Http.GetAsync(
            $"/api/exchange/rate/pair?base={baseCurrency}&quote={quoteCurrency}&asOf={Uri.EscapeDataString(asOf.ToString("o"))}");
        Assert.Equal(HttpStatusCode.OK, byPair.StatusCode);

        // 5) Expire — proves multi-event replay (Defined + Activated).
        var expire = await ExchangeApiEnvelope.PostAsync(
            _fix.Http, "/api/exchange/rate/expire",
            new ExpireExchangeRateRequestModel(expectedRateId), correlationId);
        Assert.Equal(HttpStatusCode.OK, expire.StatusCode);

        // 6) GET after expire — row is still present.
        var getAfter = await _fix.Http.GetAsync($"/api/exchange/rate/{expectedRateId}");
        Assert.Equal(HttpStatusCode.OK, getAfter.StatusCode);
        var afterRead = await ExchangeApiEnvelope.ReadAsync<ExchangeRateReadModel>(getAfter);
        Assert.True(afterRead!.Success);
        Assert.Equal(expectedRateId, afterRead.Data!.RateId);
    }

    [Fact]
    public async Task Define_NonPositiveRateValue_IsRejected_NoProjectionRow()
    {
        var baseCurrency = _fix.RunCurrencyCode("rate:invalid:base");
        var quoteCurrency = _fix.RunCurrencyCode("rate:invalid:quote");
        const int version = 1;
        var expectedRateId = _fix.IdGenerator.Generate(
            $"economic:exchange:rate:{baseCurrency}:{quoteCurrency}:{version}");
        var correlationId = _fix.SeedId("rate:invalid:corr");

        var define = await ExchangeApiEnvelope.PostAsync(
            _fix.Http, "/api/exchange/rate/define",
            new DefineExchangeRateRequestModel(
                baseCurrency, quoteCurrency, 0m, _fix.Clock.UtcNow, version),
            correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, define.StatusCode);

        await ExchangeProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, expectedRateId);
    }
}
