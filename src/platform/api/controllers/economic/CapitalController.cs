using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic;

[Authorize]
[ApiController]
[Route("api/capital")]
[ApiExplorerSettings(GroupName = "economic.capital")]
public sealed class CapitalController : ControllerBase
{
    private static readonly DomainRoute AccountRoute = new("economic", "capital", "account");
    private static readonly DomainRoute AllocationRoute = new("economic", "capital", "allocation");
    private static readonly DomainRoute AssetRoute = new("economic", "capital", "asset");
    private static readonly DomainRoute BindingRoute = new("economic", "capital", "binding");
    private static readonly DomainRoute PoolRoute = new("economic", "capital", "pool");
    private static readonly DomainRoute ReserveRoute = new("economic", "capital", "reserve");
    private static readonly DomainRoute VaultRoute = new("economic", "capital", "vault");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public CapitalController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── account ─────────────────────────────────────────────────

    [HttpPost("account/open")]
    public Task<IActionResult> OpenAccount([FromBody] ApiRequest<OpenAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var accountId = _idGenerator.Generate($"economic:capital:account:{p.OwnerId}:{p.Currency}");
        var cmd = new OpenCapitalAccountCommand(accountId, p.OwnerId, p.Currency, _clock.UtcNow);
        return Dispatch(cmd, AccountRoute, "capital_account_opened", "economic.capital.account.open_failed", ct);
    }

    [HttpPost("account/credit")]
    public Task<IActionResult> Credit([FromBody] ApiRequest<CreditAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FundCapitalAccountCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_credited", "economic.capital.account.credit_failed", ct);
    }

    [HttpPost("account/debit")]
    public Task<IActionResult> Debit([FromBody] ApiRequest<DebitAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AllocateCapitalAccountCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_debited", "economic.capital.account.debit_failed", ct);
    }

    [HttpPost("account/reserve")]
    public Task<IActionResult> ReserveAccount([FromBody] ApiRequest<ReserveAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReserveCapitalAccountCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_reserved", "economic.capital.account.reserve_failed", ct);
    }

    [HttpPost("account/release")]
    public Task<IActionResult> ReleaseAccount([FromBody] ApiRequest<ReleaseAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseCapitalReservationCommand(p.AccountId, p.Amount, p.Currency);
        return Dispatch(cmd, AccountRoute, "capital_account_released", "economic.capital.account.release_failed", ct);
    }

    [HttpPost("account/freeze")]
    public Task<IActionResult> FreezeAccount([FromBody] ApiRequest<FreezeAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FreezeCapitalAccountCommand(p.AccountId, p.Reason);
        return Dispatch(cmd, AccountRoute, "capital_account_frozen", "economic.capital.account.freeze_failed", ct);
    }

    [HttpPost("account/close")]
    public Task<IActionResult> CloseAccount([FromBody] ApiRequest<CloseAccountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CloseCapitalAccountCommand(p.AccountId, _clock.UtcNow);
        return Dispatch(cmd, AccountRoute, "capital_account_closed", "economic.capital.account.close_failed", ct);
    }

    // ── allocation ──────────────────────────────────────────────

    [HttpPost("allocation/create")]
    public Task<IActionResult> CreateAllocation([FromBody] ApiRequest<CreateAllocationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var allocationId = _idGenerator.Generate($"economic:capital:allocation:{p.SourceAccountId}:{p.TargetId}:{p.Amount}");
        var cmd = new CreateCapitalAllocationCommand(allocationId, p.SourceAccountId, p.TargetId, p.Amount, p.Currency, _clock.UtcNow);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_created", "economic.capital.allocation.create_failed", ct);
    }

    [HttpPost("allocation/release")]
    public Task<IActionResult> ReleaseAllocation([FromBody] ApiRequest<ReleaseAllocationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseCapitalAllocationCommand(p.AllocationId, _clock.UtcNow);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_released", "economic.capital.allocation.release_failed", ct);
    }

    [HttpPost("allocation/complete")]
    public Task<IActionResult> CompleteAllocation([FromBody] ApiRequest<CompleteAllocationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CompleteCapitalAllocationCommand(p.AllocationId, _clock.UtcNow);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_completed", "economic.capital.allocation.complete_failed", ct);
    }

    [HttpPost("allocation/spv")]
    public Task<IActionResult> AllocateToSpv([FromBody] ApiRequest<AllocateToSpvRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AllocateCapitalToSpvCommand(p.AllocationId, p.SpvTargetId, p.OwnershipPercentage);
        return Dispatch(cmd, AllocationRoute, "capital_allocation_spv_declared", "economic.capital.allocation.spv_failed", ct);
    }

    // ── asset ───────────────────────────────────────────────────

    [HttpPost("asset/create")]
    public Task<IActionResult> CreateAsset([FromBody] ApiRequest<CreateAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var assetId = _idGenerator.Generate($"economic:capital:asset:{p.OwnerId}:{p.InitialValue}:{p.Currency}");
        var cmd = new CreateAssetCommand(assetId, p.OwnerId, p.InitialValue, p.Currency, _clock.UtcNow);
        return Dispatch(cmd, AssetRoute, "capital_asset_created", "economic.capital.asset.create_failed", ct);
    }

    [HttpPost("asset/revalue")]
    public Task<IActionResult> RevalueAsset([FromBody] ApiRequest<RevalueAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new RevalueAssetCommand(p.AssetId, p.NewValue, _clock.UtcNow);
        return Dispatch(cmd, AssetRoute, "capital_asset_revalued", "economic.capital.asset.revalue_failed", ct);
    }

    [HttpPost("asset/dispose")]
    public Task<IActionResult> DisposeAsset([FromBody] ApiRequest<DisposeAssetRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DisposeAssetCommand(p.AssetId, _clock.UtcNow);
        return Dispatch(cmd, AssetRoute, "capital_asset_disposed", "economic.capital.asset.dispose_failed", ct);
    }

    // ── binding ─────────────────────────────────────────────────

    [HttpPost("binding/bind")]
    public Task<IActionResult> Bind([FromBody] ApiRequest<BindRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var bindingId = _idGenerator.Generate($"economic:capital:binding:{p.AccountId}:{p.OwnerId}");
        var cmd = new BindCapitalCommand(bindingId, p.AccountId, p.OwnerId, p.OwnershipType, _clock.UtcNow);
        return Dispatch(cmd, BindingRoute, "capital_bound", "economic.capital.binding.bind_failed", ct);
    }

    [HttpPost("binding/transfer")]
    public Task<IActionResult> TransferBinding([FromBody] ApiRequest<TransferBindingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new TransferBindingOwnershipCommand(p.BindingId, p.NewOwnerId, p.NewOwnershipType, _clock.UtcNow);
        return Dispatch(cmd, BindingRoute, "capital_binding_transferred", "economic.capital.binding.transfer_failed", ct);
    }

    [HttpPost("binding/release")]
    public Task<IActionResult> ReleaseBinding([FromBody] ApiRequest<ReleaseBindingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseBindingCommand(p.BindingId, _clock.UtcNow);
        return Dispatch(cmd, BindingRoute, "capital_binding_released", "economic.capital.binding.release_failed", ct);
    }

    // ── pool ────────────────────────────────────────────────────

    [HttpPost("pool/create")]
    public Task<IActionResult> CreatePool([FromBody] ApiRequest<CreatePoolRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var poolId = _idGenerator.Generate($"economic:capital:pool:{p.Currency}");
        var cmd = new CreateCapitalPoolCommand(poolId, p.Currency, _clock.UtcNow);
        return Dispatch(cmd, PoolRoute, "capital_pool_created", "economic.capital.pool.create_failed", ct);
    }

    [HttpPost("pool/aggregate")]
    public Task<IActionResult> AggregateToPool([FromBody] ApiRequest<AggregateToPoolRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AggregateCapitalToPoolCommand(p.PoolId, p.SourceAccountId, p.Amount);
        return Dispatch(cmd, PoolRoute, "capital_pool_aggregated", "economic.capital.pool.aggregate_failed", ct);
    }

    [HttpPost("pool/reduce")]
    public Task<IActionResult> ReduceFromPool([FromBody] ApiRequest<ReduceFromPoolRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReduceCapitalFromPoolCommand(p.PoolId, p.SourceAccountId, p.Amount);
        return Dispatch(cmd, PoolRoute, "capital_pool_reduced", "economic.capital.pool.reduce_failed", ct);
    }

    // ── reserve ─────────────────────────────────────────────────

    [HttpPost("reserve/create")]
    public Task<IActionResult> CreateReserve([FromBody] ApiRequest<CreateReserveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var reserveId = _idGenerator.Generate($"economic:capital:reserve:{p.AccountId}:{p.Amount}:{p.Currency}");
        var cmd = new CreateCapitalReserveCommand(reserveId, p.AccountId, p.Amount, p.Currency, _clock.UtcNow, p.ExpiresAt);
        return Dispatch(cmd, ReserveRoute, "capital_reserve_created", "economic.capital.reserve.create_failed", ct);
    }

    [HttpPost("reserve/release")]
    public Task<IActionResult> ReleaseReserve([FromBody] ApiRequest<ReleaseReserveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseCapitalReserveCommand(p.ReserveId, _clock.UtcNow);
        return Dispatch(cmd, ReserveRoute, "capital_reserve_released", "economic.capital.reserve.release_failed", ct);
    }

    [HttpPost("reserve/expire")]
    public Task<IActionResult> ExpireReserve([FromBody] ApiRequest<ExpireReserveRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ExpireCapitalReserveCommand(p.ReserveId, _clock.UtcNow);
        return Dispatch(cmd, ReserveRoute, "capital_reserve_expired", "economic.capital.reserve.expire_failed", ct);
    }

    // ── vault ───────────────────────────────────────────────────

    [HttpPost("vault/create")]
    public Task<IActionResult> CreateVault([FromBody] ApiRequest<CreateVaultRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var vaultId = _idGenerator.Generate($"economic:capital:vault:{p.OwnerId}:{p.Currency}");
        var cmd = new CreateCapitalVaultCommand(vaultId, p.OwnerId, p.Currency, _clock.UtcNow);
        return Dispatch(cmd, VaultRoute, "capital_vault_created", "economic.capital.vault.create_failed", ct);
    }

    [HttpPost("vault/slice/add")]
    public Task<IActionResult> AddVaultSlice([FromBody] ApiRequest<AddVaultSliceRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var sliceId = _idGenerator.Generate($"economic:capital:vault:{p.VaultId}:slice:{p.TotalCapacity}:{p.Currency}");
        var cmd = new AddCapitalVaultSliceCommand(p.VaultId, sliceId, p.TotalCapacity, p.Currency);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_added", "economic.capital.vault.slice_add_failed", ct);
    }

    [HttpPost("vault/slice/deposit")]
    public Task<IActionResult> DepositVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DepositToCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_deposited", "economic.capital.vault.slice_deposit_failed", ct);
    }

    [HttpPost("vault/slice/allocate")]
    public Task<IActionResult> AllocateVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AllocateFromCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_allocated", "economic.capital.vault.slice_allocate_failed", ct);
    }

    [HttpPost("vault/slice/release")]
    public Task<IActionResult> ReleaseVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseToCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_released", "economic.capital.vault.slice_release_failed", ct);
    }

    [HttpPost("vault/slice/withdraw")]
    public Task<IActionResult> WithdrawVaultSlice([FromBody] ApiRequest<SliceAmountRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new WithdrawFromCapitalVaultSliceCommand(p.VaultId, p.SliceId, p.Amount);
        return Dispatch(cmd, VaultRoute, "capital_vault_slice_withdrawn", "economic.capital.vault.slice_withdraw_failed", ct);
    }

    // ── helper ──────────────────────────────────────────────────

    private async Task<IActionResult> Dispatch(object command, DomainRoute route, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record OpenAccountRequestModel(Guid OwnerId, string Currency);
public sealed record CreditAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record DebitAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record ReserveAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record ReleaseAccountRequestModel(Guid AccountId, decimal Amount, string Currency);
public sealed record FreezeAccountRequestModel(Guid AccountId, string Reason);
public sealed record CloseAccountRequestModel(Guid AccountId);

public sealed record CreateAllocationRequestModel(Guid SourceAccountId, Guid TargetId, decimal Amount, string Currency);
public sealed record ReleaseAllocationRequestModel(Guid AllocationId);
public sealed record CompleteAllocationRequestModel(Guid AllocationId);
public sealed record AllocateToSpvRequestModel(Guid AllocationId, string SpvTargetId, decimal OwnershipPercentage);

public sealed record CreateAssetRequestModel(Guid OwnerId, decimal InitialValue, string Currency);
public sealed record RevalueAssetRequestModel(Guid AssetId, decimal NewValue);
public sealed record DisposeAssetRequestModel(Guid AssetId);

public sealed record BindRequestModel(Guid AccountId, Guid OwnerId, int OwnershipType);
public sealed record TransferBindingRequestModel(Guid BindingId, Guid NewOwnerId, int NewOwnershipType);
public sealed record ReleaseBindingRequestModel(Guid BindingId);

public sealed record CreatePoolRequestModel(string Currency);
public sealed record AggregateToPoolRequestModel(Guid PoolId, Guid SourceAccountId, decimal Amount);
public sealed record ReduceFromPoolRequestModel(Guid PoolId, Guid SourceAccountId, decimal Amount);

public sealed record CreateReserveRequestModel(Guid AccountId, decimal Amount, string Currency, DateTimeOffset ExpiresAt);
public sealed record ReleaseReserveRequestModel(Guid ReserveId);
public sealed record ExpireReserveRequestModel(Guid ReserveId);

public sealed record CreateVaultRequestModel(Guid OwnerId, string Currency);
public sealed record AddVaultSliceRequestModel(Guid VaultId, decimal TotalCapacity, string Currency);
public sealed record SliceAmountRequestModel(Guid VaultId, Guid SliceId, decimal Amount);
