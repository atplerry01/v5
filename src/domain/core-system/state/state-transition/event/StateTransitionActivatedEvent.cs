using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed record StateTransitionActivatedEvent(
    [property: JsonPropertyName("AggregateId")] StateTransitionId TransitionId) : DomainEvent;
