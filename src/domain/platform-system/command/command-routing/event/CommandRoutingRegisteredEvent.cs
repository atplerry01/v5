using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandRouting;

public sealed record CommandRoutingRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] CommandRoutingRuleId CommandRoutingRuleId,
    CommandTypeRef CommandTypeRef,
    DomainRoute HandlerRoute,
    Timestamp RegisteredAt) : DomainEvent;
