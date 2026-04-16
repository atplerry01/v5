using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Capital.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Capital.Asset;

[Authorize]
[ApiController]
[Route("api/capital/asset")]
[ApiExplorerSettings(GroupName = "economic.capital.asset")]
public sealed class AssetController : CapitalControllerBase
{
    private static readonly DomainRoute AssetRoute = new("economic", "capital", "asset");

    public AssetController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> CreateAsset([FromBody] ApiRequest<CreateAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var assetId = IdGenerator.Generate($"economic:capital:asset:{p.OwnerId}:{p.InitialValue}:{p.Currency}");
        var cmd = new CreateAssetCommand(assetId, p.OwnerId, p.InitialValue, p.Currency, Clock.UtcNow);
        return Dispatch(cmd, AssetRoute, "capital_asset_created", "economic.capital.asset.create_failed", ct);
    }

    [HttpPost("revalue")]
    public Task<IActionResult> RevalueAsset([FromBody] ApiRequest<RevalueAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RevalueAssetCommand(p.AssetId, p.NewValue, Clock.UtcNow);
        return Dispatch(cmd, AssetRoute, "capital_asset_revalued", "economic.capital.asset.revalue_failed", ct);
    }

    [HttpPost("dispose")]
    public Task<IActionResult> DisposeAsset([FromBody] ApiRequest<DisposeAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DisposeAssetCommand(p.AssetId, Clock.UtcNow);
        return Dispatch(cmd, AssetRoute, "capital_asset_disposed", "economic.capital.asset.dispose_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetAsset(Guid id, CancellationToken ct) =>
        LoadReadModel<CapitalAssetReadModel>(
            id,
            "projection_economic_capital_asset",
            "capital_asset_read_model",
            "economic.capital.asset.not_found",
            ct);
}

public sealed record CreateAssetRequestModel(Guid OwnerId, decimal InitialValue, string Currency);
public sealed record RevalueAssetRequestModel(Guid AssetId, decimal NewValue);
public sealed record DisposeAssetRequestModel(Guid AssetId);
