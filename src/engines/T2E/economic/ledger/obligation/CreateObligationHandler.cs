using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Ledger.Obligation;

public sealed class CreateObligationHandler : IEngine
{
    private readonly IClock _clock;

    public CreateObligationHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateObligationCommand cmd)
            return Task.CompletedTask;

        var obligation = ObligationAggregate.Create(
            new ObligationId(cmd.ObligationId),
            cmd.CounterpartyId,
            ParseType(cmd.Type),
            new Amount(cmd.Amount),
            new Currency(cmd.Currency),
            new Timestamp(_clock.UtcNow));

        context.EmitEvents(obligation.DomainEvents);
        return Task.CompletedTask;
    }

    private static ObligationType ParseType(string type) =>
        type switch
        {
            "Payable" => ObligationType.Payable,
            "Receivable" => ObligationType.Receivable,
            _ => throw new ArgumentException($"Unknown obligation type '{type}'.", nameof(type))
        };
}
