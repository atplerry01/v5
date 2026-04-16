using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Ledger.Treasury;

public sealed class CreateTreasuryHandler : IEngine
{
    private readonly IClock _clock;

    public CreateTreasuryHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateTreasuryCommand cmd)
            return Task.CompletedTask;

        var treasury = TreasuryAggregate.Create(
            new TreasuryId(cmd.TreasuryId),
            new Currency(cmd.Currency),
            new Timestamp(_clock.UtcNow));

        context.EmitEvents(treasury.DomainEvents);
        return Task.CompletedTask;
    }
}
