using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public sealed record ClauseActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ClauseId ClauseId) : DomainEvent;
