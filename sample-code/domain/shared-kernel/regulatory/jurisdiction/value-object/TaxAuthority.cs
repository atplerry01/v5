namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed record TaxAuthority
{
    public string Code { get; }
    public string Name { get; }

    public TaxAuthority(string code, string name)
    {
        Guard.AgainstEmpty(code, nameof(code));
        Guard.AgainstEmpty(name, nameof(name));

        Code = code;
        Name = name;
    }
}
