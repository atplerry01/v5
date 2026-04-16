using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Capital.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Capital.Reserve;

[Authorize]
[ApiController]
[Route("api/capital/reserve")]
[ApiExplorerSettings(GroupName = "economic.capital.reserve")]
public sealed class ReserveController : CapitalControllerBase
{
    private static readonly DomainRoute ReserveRoute = new("economic", "capital", "reserve");

    public ReserveController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> CreateReserve([FromBody] ApiRequest<CreateReserveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var reserveId = IdGenerator.Generate($"economic:capital:reserve:{p.AccountId}:{p.Amount}:{p.Currency}");
        var cmd = new CreateCapitalReserveCommand(reserveId, p.AccountId, p.Amount, p.Currency, Clock.UtcNow, p.ExpiresAt);
        return Dispatch(cmd, ReserveRoute, "capital_reserve_created", "economic.capital.reserve.create_failed", ct);
    }

    [HttpPost("release")]
    public Task<IActionResult> ReleaseReserve([FromBody] ApiRequest<ReleaseReserveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseCapitalReserveCommand(p.ReserveId, Clock.UtcNow);
        return Dispatch(cmd, ReserveRoute, "capital_reserve_released", "economic.capital.reserve.release_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> ExpireReserve([FromBody] ApiRequest<ExpireReserveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ExpireCapitalReserveCommand(p.ReserveId, Clock.UtcNow);
        return Dispatch(cmd, ReserveRoute, "capital_reserve_expired", "economic.capital.reserve.expire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetReserve(Guid id, CancellationToken ct) =>
        LoadReadModel<CapitalReserveReadModel>(
            id,
            "projection_economic_capital_reserve",
            "capital_reserve_read_model",
            "economic.capital.reserve.not_found",
            ct);
}

public sealed record CreateReserveRequestModel(Guid AccountId, decimal Amount, string Currency, DateTimeOffset ExpiresAt);
public sealed record ReleaseReserveRequestModel(Guid ReserveId);
public sealed record ExpireReserveRequestModel(Guid ReserveId);
