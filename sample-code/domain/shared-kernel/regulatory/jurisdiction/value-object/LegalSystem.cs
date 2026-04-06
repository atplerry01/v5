namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed record LegalSystem
{
    public string Value { get; }

    public LegalSystem(string value)
    {
        Guard.AgainstEmpty(value, nameof(value));
        Value = value;
    }

    public static readonly LegalSystem CommonLaw = new("common_law");
    public static readonly LegalSystem CivilLaw = new("civil_law");
    public static readonly LegalSystem ReligiousLaw = new("religious_law");
    public static readonly LegalSystem CustomaryLaw = new("customary_law");
    public static readonly LegalSystem Mixed = new("mixed");
}