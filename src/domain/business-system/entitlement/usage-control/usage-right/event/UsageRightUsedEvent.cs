using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed record UsageRightUsedEvent(
    [property: JsonPropertyName("AggregateId")] UsageRightId UsageRightId,
    UsageRecordId RecordId,
    int UnitsUsed) : DomainEvent;
