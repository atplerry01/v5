using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Document.Record;

public sealed record RecordCreatedEvent(Guid RecordId) : DomainEvent;
