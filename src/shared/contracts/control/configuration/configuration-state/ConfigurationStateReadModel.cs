namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;

public sealed record ConfigurationStateReadModel
{
    public Guid StateId { get; init; }
    public string DefinitionId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public int Version { get; init; }
    public bool IsRevoked { get; init; }
}
