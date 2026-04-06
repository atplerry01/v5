namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed class CrossSpvException : DomainException
{
    public CrossSpvException(string message) : base("CROSS_SPV_ERROR", message) { }
}
