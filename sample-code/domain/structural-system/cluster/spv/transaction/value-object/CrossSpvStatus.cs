namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record CrossSpvStatus(string Value)
{
    public static readonly CrossSpvStatus Created = new("created");
    public static readonly CrossSpvStatus Prepared = new("prepared");
    public static readonly CrossSpvStatus Committed = new("committed");
    public static readonly CrossSpvStatus Failed = new("failed");

    public bool IsTerminal => this == Committed || this == Failed;
}
