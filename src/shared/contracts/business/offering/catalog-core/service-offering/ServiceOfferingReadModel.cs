namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;

public sealed record ServiceOfferingReadModel
{
    public Guid ServiceOfferingId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid ServiceDefinitionId { get; init; }
    public Guid? PackageId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
