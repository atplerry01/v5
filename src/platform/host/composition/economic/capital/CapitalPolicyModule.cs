using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Capital;

/// <summary>
/// E5.1 — capital context policy bindings. Registers one
/// <see cref="CommandPolicyBinding"/> per capital command, mapping the command
/// CLR type to its canonical policy id constant declared on the matching
/// <c>Capital{Domain}PolicyIds</c> class. The bindings are aggregated by
/// <see cref="ICommandPolicyIdRegistry"/> at runtime composition; once
/// registered, every dispatch of a capital command stamps the correct policy
/// id onto <c>CommandContext.PolicyId</c> for evaluation by
/// <c>PolicyMiddleware</c>.
///
/// Coverage: 29 commands → 29 unique policy ids. Matches the E5 README map.
/// </summary>
public static class CapitalPolicyModule
{
    public static IServiceCollection AddCapitalPolicyBindings(this IServiceCollection services)
    {
        // ── account (7) ────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(OpenCapitalAccountCommand),       CapitalAccountPolicyIds.Open));
        services.AddSingleton(new CommandPolicyBinding(typeof(FundCapitalAccountCommand),       CapitalAccountPolicyIds.Fund));
        services.AddSingleton(new CommandPolicyBinding(typeof(AllocateCapitalAccountCommand),   CapitalAccountPolicyIds.Allocate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReserveCapitalAccountCommand),    CapitalAccountPolicyIds.Reserve));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseCapitalReservationCommand), CapitalAccountPolicyIds.ReleaseReservation));
        services.AddSingleton(new CommandPolicyBinding(typeof(FreezeCapitalAccountCommand),     CapitalAccountPolicyIds.Freeze));
        services.AddSingleton(new CommandPolicyBinding(typeof(CloseCapitalAccountCommand),      CapitalAccountPolicyIds.Close));

        // ── allocation (4) ─────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateCapitalAllocationCommand),   CapitalAllocationPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseCapitalAllocationCommand),  CapitalAllocationPolicyIds.Release));
        services.AddSingleton(new CommandPolicyBinding(typeof(CompleteCapitalAllocationCommand), CapitalAllocationPolicyIds.Complete));
        services.AddSingleton(new CommandPolicyBinding(typeof(AllocateCapitalToSpvCommand),      CapitalAllocationPolicyIds.SpvDeclare));

        // ── asset (3) ──────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateAssetCommand),  CapitalAssetPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevalueAssetCommand), CapitalAssetPolicyIds.Revalue));
        services.AddSingleton(new CommandPolicyBinding(typeof(DisposeAssetCommand), CapitalAssetPolicyIds.Dispose));

        // ── binding (3) ────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(BindCapitalCommand),               CapitalBindingPolicyIds.Bind));
        services.AddSingleton(new CommandPolicyBinding(typeof(TransferBindingOwnershipCommand),  CapitalBindingPolicyIds.Transfer));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseBindingCommand),            CapitalBindingPolicyIds.Release));

        // ── pool (3) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateCapitalPoolCommand),       CapitalPoolPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AggregateCapitalToPoolCommand),  CapitalPoolPolicyIds.Aggregate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReduceCapitalFromPoolCommand),   CapitalPoolPolicyIds.Reduce));

        // ── reserve (3) ────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateCapitalReserveCommand),  CapitalReservePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseCapitalReserveCommand), CapitalReservePolicyIds.Release));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireCapitalReserveCommand),  CapitalReservePolicyIds.Expire));

        // ── vault (6) ──────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateCapitalVaultCommand),            CapitalVaultPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AddCapitalVaultSliceCommand),          CapitalVaultPolicyIds.SliceAdd));
        services.AddSingleton(new CommandPolicyBinding(typeof(DepositToCapitalVaultSliceCommand),    CapitalVaultPolicyIds.SliceDeposit));
        services.AddSingleton(new CommandPolicyBinding(typeof(AllocateFromCapitalVaultSliceCommand), CapitalVaultPolicyIds.SliceAllocate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseToCapitalVaultSliceCommand),    CapitalVaultPolicyIds.SliceRelease));
        services.AddSingleton(new CommandPolicyBinding(typeof(WithdrawFromCapitalVaultSliceCommand), CapitalVaultPolicyIds.SliceWithdraw));

        return services;
    }
}
