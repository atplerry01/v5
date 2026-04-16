namespace Whycespace.Shared.Contracts.Events.Economic.Subject.Subject;

public sealed record EconomicSubjectRegisteredEventSchema(
    Guid AggregateId,
    string SubjectType,
    string StructuralRefType,
    string StructuralRefId,
    string EconomicRefType,
    string EconomicRefId);
