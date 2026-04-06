namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class SpvException : DomainException
{
    public SpvException(string message) : base("SPV_ERROR", message) { }
}
