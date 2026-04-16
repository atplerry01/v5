using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Exchange.Fx;

public sealed record RegisterFxPairCommand(
    Guid FxId,
    string BaseCurrency,
    string QuoteCurrency) : IHasAggregateId
{
    public Guid AggregateId => FxId;
}

public sealed record ActivateFxPairCommand(
    Guid FxId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => FxId;
}

public sealed record DeactivateFxPairCommand(
    Guid FxId,
    DateTimeOffset DeactivatedAt) : IHasAggregateId
{
    public Guid AggregateId => FxId;
}
