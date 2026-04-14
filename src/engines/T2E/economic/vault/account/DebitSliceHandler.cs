using Whycespace.Domain.EconomicSystem.Vault.Account;
using Whycespace.Domain.EconomicSystem.Vault.Slice;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Vault.Account;

public sealed class DebitSliceHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DebitSliceCommand cmd)
            return;

        var aggregate = (VaultAccountAggregate)await context.LoadAggregateAsync(typeof(VaultAccountAggregate));
        aggregate.DebitSlice(MapSlice(cmd.Slice), new Amount(cmd.Amount));
        context.EmitEvents(aggregate.DomainEvents);
    }

    private static SliceType MapSlice(VaultSliceType slice) => slice switch
    {
        VaultSliceType.Slice1 => SliceType.Slice1,
        VaultSliceType.Slice2 => SliceType.Slice2,
        VaultSliceType.Slice3 => SliceType.Slice3,
        VaultSliceType.Slice4 => SliceType.Slice4,
        _ => throw new ArgumentOutOfRangeException(nameof(slice), slice, "Unknown slice.")
    };
}
