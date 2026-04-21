using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public sealed record RenewalCreatedEvent(
    [property: JsonPropertyName("AggregateId")] RenewalId RenewalId,
    RenewalSourceId SourceId) : DomainEvent;
