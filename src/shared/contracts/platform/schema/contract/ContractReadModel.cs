namespace Whycespace.Shared.Contracts.Platform.Schema.Contract;

public sealed record ContractReadModel
{
    public Guid ContractId { get; init; }
    public string ContractName { get; init; } = string.Empty;
    public string PublisherClassification { get; init; } = string.Empty;
    public string PublisherContext { get; init; } = string.Empty;
    public string PublisherDomain { get; init; } = string.Empty;
    public Guid SchemaRef { get; init; }
    public int SchemaVersion { get; init; }
    public int SubscriberCount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
