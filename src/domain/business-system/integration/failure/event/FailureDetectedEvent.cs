namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public sealed record FailureDetectedEvent(FailureId FailureId, FailureTypeId TypeId);
