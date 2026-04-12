namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public sealed record FailureClassifiedEvent(FailureId FailureId, string Classification);
