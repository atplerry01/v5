using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Certification;

public sealed record CertificateSerial : ValueObject
{
    public string Value { get; }
    private CertificateSerial(string v) => Value = v;
    public static CertificateSerial Create(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) throw CertificationErrors.InvalidSerial();
        return new CertificateSerial(v.Trim());
    }
    public override string ToString() => Value;
}
