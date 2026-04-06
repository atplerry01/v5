namespace Whycespace.Domain.StructuralSystem.HumanCapital.Operator;

public sealed class Certification
{
    public Guid Id { get; init; }
    public string CertificationType { get; init; } = string.Empty;
    public DateTimeOffset IssuedAt { get; init; }
}
