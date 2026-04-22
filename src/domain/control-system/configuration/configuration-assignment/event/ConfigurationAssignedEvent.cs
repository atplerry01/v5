using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;

public sealed record ConfigurationAssignedEvent(
    ConfigurationAssignmentId Id,
    string DefinitionId,
    string ScopeId,
    string Value) : DomainEvent;
