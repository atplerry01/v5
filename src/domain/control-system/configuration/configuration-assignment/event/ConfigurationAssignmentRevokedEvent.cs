using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;

public sealed record ConfigurationAssignmentRevokedEvent(
    ConfigurationAssignmentId Id) : DomainEvent;
