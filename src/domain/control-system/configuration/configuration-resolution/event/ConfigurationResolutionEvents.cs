using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationResolution;

public sealed record ConfigurationResolvedEvent(
    ConfigurationResolutionId Id,
    string DefinitionId,
    string ScopeId,
    string StateId,
    string ResolvedValue,
    DateTimeOffset ResolvedAt) : DomainEvent;
