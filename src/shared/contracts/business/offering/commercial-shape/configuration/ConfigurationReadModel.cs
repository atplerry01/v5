namespace Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;

public sealed record ConfigurationReadModel
{
    public Guid ConfigurationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<ConfigurationOptionReadModel> Options { get; init; } = Array.Empty<ConfigurationOptionReadModel>();
}

public sealed record ConfigurationOptionReadModel(string Key, string Value);
