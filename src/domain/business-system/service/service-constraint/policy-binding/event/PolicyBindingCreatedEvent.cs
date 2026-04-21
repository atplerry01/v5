using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed record PolicyBindingCreatedEvent(
    [property: JsonPropertyName("AggregateId")] PolicyBindingId PolicyBindingId,
    ServiceDefinitionRef ServiceDefinition,
    PolicyRef Policy,
    PolicyBindingScope Scope) : DomainEvent;
