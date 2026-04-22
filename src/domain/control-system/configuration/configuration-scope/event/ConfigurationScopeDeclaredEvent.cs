using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;

public sealed record ConfigurationScopeDeclaredEvent(ConfigurationScopeId Id, string DefinitionId, string Classification, string? Context) : DomainEvent;
