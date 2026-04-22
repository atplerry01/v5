using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;

public sealed record ConfigurationScopeRemovedEvent(ConfigurationScopeId Id) : DomainEvent;
