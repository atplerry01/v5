namespace Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Account;

public sealed record AccountReadModel
{
    public Guid AccountId { get; init; }
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
