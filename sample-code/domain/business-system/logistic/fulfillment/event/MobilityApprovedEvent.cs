using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed record MobilityApprovedEvent(Guid RequestId, Guid RequestorIdentityId) : DomainEvent;
