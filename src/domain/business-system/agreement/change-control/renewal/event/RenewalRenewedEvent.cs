using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public sealed record RenewalRenewedEvent(
    [property: JsonPropertyName("AggregateId")] RenewalId RenewalId) : DomainEvent;
