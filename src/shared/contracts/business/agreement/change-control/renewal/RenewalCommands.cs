using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;

public sealed record CreateRenewalCommand(Guid RenewalId, Guid SourceId) : IHasAggregateId
{
    public Guid AggregateId => RenewalId;
}

public sealed record RenewRenewalCommand(Guid RenewalId) : IHasAggregateId
{
    public Guid AggregateId => RenewalId;
}

public sealed record ExpireRenewalCommand(Guid RenewalId) : IHasAggregateId
{
    public Guid AggregateId => RenewalId;
}
