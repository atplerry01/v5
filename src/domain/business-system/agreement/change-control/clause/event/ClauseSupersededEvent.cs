using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public sealed record ClauseSupersededEvent(
    [property: JsonPropertyName("AggregateId")] ClauseId ClauseId) : DomainEvent;
