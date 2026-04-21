using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public sealed record LimitCreatedEvent(
    [property: JsonPropertyName("AggregateId")] LimitId LimitId,
    LimitSubjectId SubjectId,
    int ThresholdValue) : DomainEvent;
