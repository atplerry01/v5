namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public sealed record MilestoneCreatedEvent(MilestoneId MilestoneId, MilestoneTargetId TargetId);
