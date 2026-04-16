using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Capital.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Capital.Vault;

[Authorize]
[ApiController]
[Route("api/capital/vault")]
[ApiExplorerSettings(GroupName = "economic.capital.vault")]
public sealed class VaultController : CapitalControllerBase
{
    private static readonly DomainRoute VaultRoute = new("economic", "capital", "vault");

    public VaultController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> CreateVault([FromBody] ApiRequest<CreateVaultRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var vaultId = IdGenerator.Generate($"economic:capital:vault:{p.OwnerId}:{p.Currency}");
        var cmd = new CreateCapitalVaultCommand(vaultId, p.OwnerId, p.Currency, Clock.UtcNow);
        return Dispatch(cmd, VaultRoute, "capital_vault_created", "economic.capital.vault.create_failed", ct);
    }

    [HttpPost("slice/add")]
    public Task<IActionResult> AddVaultSlice([FromBody] ApiRequest<AddVaultSliceRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var sliceId = IdGenerator.Generate($"economic:capital:vault:{p.VaultId}:slice:{p.TotalCapacity}:{p.Currency}");
        var cmd = new AddCapitalVaultSliceCommand(p.VaultId, sliceId, p.TotalCapacity, p.Currency);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_added", "economic.capital.vault.slice_add_failed", ct);
    }

    [HttpPost("slice/deposit")]
    public Task<IActionResult> DepositVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DepositToCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_deposited", "economic.capital.vault.slice_deposit_failed", ct);
    }

    [HttpPost("slice/allocate")]
    public Task<IActionResult> AllocateVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AllocateFromCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_allocated", "economic.capital.vault.slice_allocate_failed", ct);
    }

    [HttpPost("slice/release")]
    public Task<IActionResult> ReleaseVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseToCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_released", "economic.capital.vault.slice_release_failed", ct);
    }

    [HttpPost("slice/withdraw")]
    public Task<IActionResult> WithdrawVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new WithdrawFromCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_withdrawn", "economic.capital.vault.slice_withdraw_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetVault(Guid id, CancellationToken ct) =>
        LoadReadModel<CapitalVaultReadModel>(
            id,
            "projection_economic_capital_vault",
            "capital_vault_read_model",
            "economic.capital.vault.not_found",
            ct);
}

public sealed record CreateVaultRequestModel(Guid OwnerId, string Currency);
public sealed record AddVaultSliceRequestModel(Guid VaultId, decimal TotalCapacity, string Currency);
public sealed record SliceAmountRequestModel(Guid VaultId, Guid SliceId, decimal Amount);
