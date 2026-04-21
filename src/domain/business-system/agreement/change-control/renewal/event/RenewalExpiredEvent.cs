using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public sealed record RenewalExpiredEvent(
    [property: JsonPropertyName("AggregateId")] RenewalId RenewalId) : DomainEvent;
