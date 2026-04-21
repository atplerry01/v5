using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed record ServiceOptionUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceOptionId ServiceOptionId,
    OptionName Name,
    OptionKind Kind) : DomainEvent;
