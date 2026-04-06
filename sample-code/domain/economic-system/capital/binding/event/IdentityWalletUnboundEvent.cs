namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed record IdentityWalletUnboundEvent(Guid BindingId, Guid IdentityId, Guid WalletId) : DomainEvent;
