namespace Whycespace.Shared.Contracts.Economic.Capital.Binding;

public sealed record CapitalBindingReadModel
{
    public Guid BindingId { get; init; }
    public Guid AccountId { get; init; }
    public Guid OwnerId { get; init; }
    public int OwnershipType { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset BoundAt { get; init; }
}
