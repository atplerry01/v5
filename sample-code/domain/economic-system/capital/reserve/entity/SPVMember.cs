namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed class SPVMember
{
    public Guid Id { get; }

    public SPVMember(Guid id)
    {
        Id = id;
    }
}
