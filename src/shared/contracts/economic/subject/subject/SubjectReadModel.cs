namespace Whycespace.Shared.Contracts.Economic.Subject.Subject;

public sealed record EconomicSubjectReadModel
{
    public Guid SubjectId { get; init; }
    public string SubjectType { get; init; } = string.Empty;
    public string StructuralRefType { get; init; } = string.Empty;
    public string StructuralRefId { get; init; } = string.Empty;
    public string EconomicRefType { get; init; } = string.Empty;
    public string EconomicRefId { get; init; } = string.Empty;
    public bool IsRegistered { get; init; }
}
