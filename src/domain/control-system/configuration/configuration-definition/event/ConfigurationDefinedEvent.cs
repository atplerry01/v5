using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;

public sealed record ConfigurationDefinedEvent(
    ConfigurationDefinitionId Id,
    string Name,
    ConfigValueType ValueType,
    string Description,
    string? DefaultValue) : DomainEvent;
