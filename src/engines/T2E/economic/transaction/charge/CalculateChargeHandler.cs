using Whycespace.Domain.EconomicSystem.Transaction.Charge;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Charge;

using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Charge;

public sealed class CalculateChargeHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CalculateChargeCommand cmd)
            return Task.CompletedTask;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        if (!Enum.TryParse<ChargeType>(cmd.Type, ignoreCase: true, out var chargeType))
            throw new ArgumentException($"Unknown charge type '{cmd.Type}'.", nameof(cmd.Type));

        var aggregate = ChargeAggregate.Calculate(
            ChargeId.From(cmd.ChargeId),
            cmd.TransactionId,
            chargeType,
            new Amount(cmd.BaseAmount),
            new Amount(cmd.ChargeAmount),
            new Currency(cmd.Currency),
            new Timestamp(cmd.CalculatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
