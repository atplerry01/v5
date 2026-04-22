using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditQuery;

public sealed record AuditQueryIssuedEvent(
    AuditQueryId Id,
    string IssuedBy,
    QueryTimeRange TimeRange,
    string? CorrelationFilter,
    string? ActorFilter) : DomainEvent;
