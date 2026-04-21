using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed record ServiceOptionCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceOptionId ServiceOptionId,
    ServiceDefinitionRef ServiceDefinition,
    OptionCode Code,
    OptionName Name,
    OptionKind Kind) : DomainEvent;
