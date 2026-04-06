using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed record EvidenceCreatedEvent(Guid EvidenceId) : DomainEvent;
