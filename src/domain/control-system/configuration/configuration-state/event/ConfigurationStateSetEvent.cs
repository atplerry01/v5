using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;

public sealed record ConfigurationStateSetEvent(ConfigurationStateId Id, string DefinitionId, string Value, int Version) : DomainEvent;
