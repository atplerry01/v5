using Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Ledger.Ledger;

public sealed class OpenLedgerHandler : IEngine
{
    private readonly IClock _clock;

    public OpenLedgerHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not OpenLedgerCommand cmd)
            return Task.CompletedTask;

        var now = new Timestamp(_clock.UtcNow);
        var ledger = LedgerAggregate.Open(
            new LedgerId(cmd.LedgerId),
            new Currency(cmd.Currency),
            now);

        context.EmitEvents(ledger.DomainEvents);
        return Task.CompletedTask;
    }
}
