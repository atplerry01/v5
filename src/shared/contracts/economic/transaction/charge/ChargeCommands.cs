using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Transaction.Charge;

public sealed record CalculateChargeCommand(
    Guid ChargeId,
    Guid TransactionId,
    string Type,
    decimal BaseAmount,
    decimal ChargeAmount,
    string Currency,
    DateTimeOffset CalculatedAt) : IHasAggregateId
{
    public Guid AggregateId => ChargeId;
}

public sealed record ApplyChargeCommand(
    Guid ChargeId,
    DateTimeOffset AppliedAt) : IHasAggregateId
{
    public Guid AggregateId => ChargeId;
}
