namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public sealed record RetryCreatedEvent(RetryId RetryId, RetryPolicyId PolicyId);
