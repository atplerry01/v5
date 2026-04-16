using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Capital.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Capital.Pool;

[Authorize]
[ApiController]
[Route("api/capital/pool")]
[ApiExplorerSettings(GroupName = "economic.capital.pool")]
public sealed class PoolController : CapitalControllerBase
{
    private static readonly DomainRoute PoolRoute = new("economic", "capital", "pool");

    public PoolController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> CreatePool([FromBody] ApiRequest<CreatePoolRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var poolId = IdGenerator.Generate($"economic:capital:pool:{p.Currency}");
        var cmd = new CreateCapitalPoolCommand(poolId, p.Currency, Clock.UtcNow);
        return Dispatch(cmd, PoolRoute, "capital_pool_created", "economic.capital.pool.create_failed", ct);
    }

    [HttpPost("aggregate")]
    public Task<IActionResult> AggregateToPool([FromBody] ApiRequest<AggregateToPoolRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AggregateCapitalToPoolCommand(p.PoolId, p.SourceAccountId, p.Amount);
        return Dispatch(cmd, PoolRoute, "capital_pool_aggregated", "economic.capital.pool.aggregate_failed", ct);
    }

    [HttpPost("reduce")]
    public Task<IActionResult> ReduceFromPool([FromBody] ApiRequest<ReduceFromPoolRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReduceCapitalFromPoolCommand(p.PoolId, p.SourceAccountId, p.Amount);
        return Dispatch(cmd, PoolRoute, "capital_pool_reduced", "economic.capital.pool.reduce_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetPool(Guid id, CancellationToken ct) =>
        LoadReadModel<CapitalPoolReadModel>(
            id,
            "projection_economic_capital_pool",
            "capital_pool_read_model",
            "economic.capital.pool.not_found",
            ct);
}

public sealed record CreatePoolRequestModel(string Currency);
public sealed record AggregateToPoolRequestModel(Guid PoolId, Guid SourceAccountId, decimal Amount);
public sealed record ReduceFromPoolRequestModel(Guid PoolId, Guid SourceAccountId, decimal Amount);
