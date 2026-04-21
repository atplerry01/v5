using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public sealed record ConfigurationCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ConfigurationId ConfigurationId,
    ConfigurationName Name) : DomainEvent;
