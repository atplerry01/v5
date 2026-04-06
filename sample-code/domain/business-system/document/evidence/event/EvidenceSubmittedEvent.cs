using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed record EvidenceSubmittedEvent(Guid EvidenceId) : DomainEvent;
