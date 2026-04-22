using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;

public sealed record ConfigurationDefinitionDeprecatedEvent(ConfigurationDefinitionId Id) : DomainEvent;
