namespace Whycespace.Shared.Contracts.Economic.Enforcement.Lock;

public sealed record GetLockByIdQuery(Guid LockId);

public sealed record GetLocksBySubjectQuery(Guid SubjectId);
