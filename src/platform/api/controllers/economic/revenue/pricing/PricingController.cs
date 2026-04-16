using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Revenue.Pricing;

[Authorize]
[ApiController]
[Route("api/economic")]
[ApiExplorerSettings(GroupName = "economic.revenue.pricing")]
public sealed class PricingController : ControllerBase
{
    private static readonly DomainRoute PricingRoute = new("economic", "revenue", "pricing");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public PricingController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("pricing/define")]
    public async Task<IActionResult> Define(
        [FromBody] ApiRequest<DefinePricingRequestModel> request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var p = request.Data;
        var pricingId = _idGenerator.Generate(
            $"economic:revenue:pricing:{p.ContractId}:{p.Model}:{p.Price}:{p.Currency}");

        var cmd = new DefinePricingCommand(pricingId, p.ContractId, p.Model, p.Price, p.Currency);
        var result = await _dispatcher.DispatchAsync(cmd, PricingRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("pricing_defined"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.pricing.define_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("pricing/{pricingId:guid}/adjust")]
    public async Task<IActionResult> Adjust(
        Guid pricingId,
        [FromBody] ApiRequest<AdjustPricingRequestModel> request,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var p = request.Data;
        var cmd = new AdjustPricingCommand(pricingId, p.NewPrice, p.Reason);
        var result = await _dispatcher.DispatchAsync(cmd, PricingRoute);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("pricing_adjusted"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.pricing.adjust_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

public sealed record DefinePricingRequestModel(
    Guid ContractId,
    string Model,
    decimal Price,
    string Currency);

public sealed record AdjustPricingRequestModel(
    decimal NewPrice,
    string Reason);
