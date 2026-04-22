using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;

public sealed record AssignConfigurationCommand(
    Guid AssignmentId,
    string DefinitionId,
    string ScopeId,
    string Value) : IHasAggregateId
{
    public Guid AggregateId => AssignmentId;
}

public sealed record RevokeConfigurationAssignmentCommand(
    Guid AssignmentId) : IHasAggregateId
{
    public Guid AggregateId => AssignmentId;
}
