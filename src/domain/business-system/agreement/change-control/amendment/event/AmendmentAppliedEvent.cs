using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public sealed record AmendmentAppliedEvent(
    [property: JsonPropertyName("AggregateId")] AmendmentId AmendmentId) : DomainEvent;
