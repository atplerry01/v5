namespace Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;

public static class EvidenceRecordErrors
{
    public static InvalidOperationException AlreadyArchived() =>
        new("Evidence record is already archived.");

    public static ArgumentException EmptyActorId() =>
        new("Evidence record must have a non-empty actor id.", nameof(EvidenceDescriptor));

    public static ArgumentException EmptySubjectId() =>
        new("Evidence record must have a non-empty subject id.", nameof(EvidenceDescriptor));
}
