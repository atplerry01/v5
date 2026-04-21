using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed record PolicyBindingUnboundEvent(
    [property: JsonPropertyName("AggregateId")] PolicyBindingId PolicyBindingId,
    DateTimeOffset UnboundAt) : DomainEvent;
