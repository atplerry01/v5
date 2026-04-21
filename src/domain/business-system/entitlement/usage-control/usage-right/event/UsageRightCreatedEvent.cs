using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed record UsageRightCreatedEvent(
    [property: JsonPropertyName("AggregateId")] UsageRightId UsageRightId,
    UsageRightSubjectId SubjectId,
    UsageRightReferenceId ReferenceId,
    int TotalUnits) : DomainEvent;
