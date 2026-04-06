namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed class BindingRecord : Entity
{
    public Guid IdentityId { get; private set; }
    public Guid WalletId { get; private set; }
    public BindingStatus Status { get; private set; }
    public DateTimeOffset BoundAt { get; private set; }

    public static BindingRecord Create(Guid id, Guid identityId, Guid walletId, DateTimeOffset timestamp)
    {
        return new BindingRecord
        {
            Id = id,
            IdentityId = identityId,
            WalletId = walletId,
            Status = BindingStatus.Active,
            BoundAt = timestamp
        };
    }
}
