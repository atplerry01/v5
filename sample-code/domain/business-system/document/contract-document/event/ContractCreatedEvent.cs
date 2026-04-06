using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed record ContractCreatedEvent(Guid ContractId) : DomainEvent;
