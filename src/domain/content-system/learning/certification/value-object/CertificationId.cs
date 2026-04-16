namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public readonly record struct CertificationId(Guid Value)
{
    public static CertificationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
