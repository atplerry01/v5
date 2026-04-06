namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed record RegulatoryFramework
{
    public string Code { get; }
    public string Name { get; }

    public RegulatoryFramework(string code, string name)
    {
        Guard.AgainstEmpty(code, nameof(code));
        Guard.AgainstEmpty(name, nameof(name));

        Code = code;
        Name = name;
    }
}