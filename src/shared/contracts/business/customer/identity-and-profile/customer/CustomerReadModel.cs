namespace Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;

public sealed record CustomerReadModel
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? ReferenceCode { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
