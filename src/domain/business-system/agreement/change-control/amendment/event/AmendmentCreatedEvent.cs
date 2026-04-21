using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public sealed record AmendmentCreatedEvent(
    [property: JsonPropertyName("AggregateId")] AmendmentId AmendmentId,
    AmendmentTargetId TargetId) : DomainEvent;
