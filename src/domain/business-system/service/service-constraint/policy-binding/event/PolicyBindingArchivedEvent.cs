using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed record PolicyBindingArchivedEvent(
    [property: JsonPropertyName("AggregateId")] PolicyBindingId PolicyBindingId) : DomainEvent;
