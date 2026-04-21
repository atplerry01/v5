namespace Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;

public sealed record ProfileReadModel
{
    public Guid ProfileId { get; init; }
    public Guid CustomerId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Descriptors { get; init; } =
        new Dictionary<string, string>();
    public DateTimeOffset LastUpdatedAt { get; init; }
}
