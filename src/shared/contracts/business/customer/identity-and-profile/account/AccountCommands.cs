using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;

public sealed record CreateAccountCommand(
    Guid AccountId,
    Guid CustomerId,
    string Name,
    string Type) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record RenameAccountCommand(
    Guid AccountId,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record ActivateAccountCommand(Guid AccountId) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record SuspendAccountCommand(Guid AccountId) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}

public sealed record CloseAccountCommand(Guid AccountId) : IHasAggregateId
{
    public Guid AggregateId => AccountId;
}
