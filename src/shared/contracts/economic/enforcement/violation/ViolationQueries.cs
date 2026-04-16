namespace Whycespace.Shared.Contracts.Economic.Enforcement.Violation;

public sealed record GetViolationByIdQuery(Guid ViolationId);

public sealed record ListViolationsByRuleQuery(Guid RuleId);

public sealed record ListViolationsBySourceQuery(Guid SourceReference);
