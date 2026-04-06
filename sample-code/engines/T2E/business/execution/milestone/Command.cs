namespace Whycespace.Engines.T2E.Business.Execution.Milestone;

public record MilestoneCommand(
    string Action,
    string EntityId,
    object Payload
);
