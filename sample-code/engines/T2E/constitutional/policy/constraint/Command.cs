namespace Whycespace.Engines.T2E.Constitutional.Policy.Constraint;

public record ConstraintCommand(
    string Action,
    string EntityId,
    object Payload
);
