namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed record ServiceType(string Value)
{
    public static readonly ServiceType API = new("API");
    public static readonly ServiceType Worker = new("Worker");
    public static readonly ServiceType Gateway = new("Gateway");
    public static readonly ServiceType Integration = new("Integration");

    public override string ToString() => Value;
}
