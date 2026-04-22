namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;

public sealed record ConfigurationDefinitionReadModel
{
    public Guid DefinitionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ValueType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? DefaultValue { get; init; }
    public bool IsDeprecated { get; init; }
}
