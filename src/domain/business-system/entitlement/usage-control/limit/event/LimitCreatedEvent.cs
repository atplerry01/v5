namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public sealed record LimitCreatedEvent(LimitId LimitId, LimitSubjectId SubjectId, int ThresholdValue);
