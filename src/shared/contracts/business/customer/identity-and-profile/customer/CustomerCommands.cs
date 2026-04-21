using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;

public sealed record CreateCustomerCommand(
    Guid CustomerId,
    string Name,
    string Type,
    string? ReferenceCode) : IHasAggregateId
{
    public Guid AggregateId => CustomerId;
}

public sealed record RenameCustomerCommand(
    Guid CustomerId,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => CustomerId;
}

public sealed record ReclassifyCustomerCommand(
    Guid CustomerId,
    string Type) : IHasAggregateId
{
    public Guid AggregateId => CustomerId;
}

public sealed record ActivateCustomerCommand(Guid CustomerId) : IHasAggregateId
{
    public Guid AggregateId => CustomerId;
}

public sealed record ArchiveCustomerCommand(Guid CustomerId) : IHasAggregateId
{
    public Guid AggregateId => CustomerId;
}
