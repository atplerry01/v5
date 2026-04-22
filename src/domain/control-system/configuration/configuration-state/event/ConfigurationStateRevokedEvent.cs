using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;

public sealed record ConfigurationStateRevokedEvent(ConfigurationStateId Id) : DomainEvent;
