namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed record BindingRejectedEvent(Guid BindingId, Guid IdentityId, Guid WalletId, string Reason) : DomainEvent;
