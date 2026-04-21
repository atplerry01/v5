using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public sealed record ServiceDefinitionActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceDefinitionId ServiceDefinitionId) : DomainEvent;
