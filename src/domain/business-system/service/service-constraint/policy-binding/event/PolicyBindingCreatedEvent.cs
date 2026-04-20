using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed record PolicyBindingCreatedEvent(
    PolicyBindingId PolicyBindingId,
    ServiceDefinitionRef ServiceDefinition,
    PolicyRef Policy,
    PolicyBindingScope Scope);
