using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustUpdatedEvent(Guid TrustScoreId) : DomainEvent;
