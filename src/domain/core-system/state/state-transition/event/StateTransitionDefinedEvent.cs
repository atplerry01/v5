using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed record StateTransitionDefinedEvent(
    [property: JsonPropertyName("AggregateId")] StateTransitionId TransitionId,
    TransitionRule Rule) : DomainEvent;
