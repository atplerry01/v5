using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public sealed record ClauseCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ClauseId ClauseId,
    ClauseType ClauseType) : DomainEvent;
