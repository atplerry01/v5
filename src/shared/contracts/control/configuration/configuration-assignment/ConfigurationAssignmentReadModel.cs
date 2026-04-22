namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;

public sealed record ConfigurationAssignmentReadModel
{
    public Guid AssignmentId { get; init; }
    public string DefinitionId { get; init; } = string.Empty;
    public string ScopeId { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
