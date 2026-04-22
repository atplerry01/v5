namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public sealed record DomainRoute(string Classification, string Context, string Domain)
{
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Classification) &&
        !string.IsNullOrWhiteSpace(Context) &&
        !string.IsNullOrWhiteSpace(Domain);

    public override string ToString() => $"{Classification}/{Context}/{Domain}";
}
