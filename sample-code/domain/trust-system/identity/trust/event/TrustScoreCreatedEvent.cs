using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustScoreCreatedEvent(Guid TrustScoreId) : DomainEvent;
