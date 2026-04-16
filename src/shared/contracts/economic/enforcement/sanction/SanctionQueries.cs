namespace Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;

public sealed record GetSanctionByIdQuery(Guid SanctionId);

public sealed record GetSanctionsBySubjectQuery(Guid SubjectId);
