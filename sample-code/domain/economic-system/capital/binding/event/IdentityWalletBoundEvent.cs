namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed record IdentityWalletBoundEvent(Guid BindingId, Guid IdentityId, Guid WalletId) : DomainEvent;
