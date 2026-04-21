using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed record ServiceOptionActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceOptionId ServiceOptionId) : DomainEvent;
