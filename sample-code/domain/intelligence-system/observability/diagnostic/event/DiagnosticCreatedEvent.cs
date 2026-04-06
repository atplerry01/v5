using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Diagnostic;

public sealed record DiagnosticCreatedEvent(Guid ReportId) : DomainEvent;
