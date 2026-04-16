namespace Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;

public sealed record GetRestrictionByIdQuery(Guid RestrictionId);

public sealed record GetRestrictionsBySubjectQuery(Guid SubjectId);
