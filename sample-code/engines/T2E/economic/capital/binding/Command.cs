namespace Whycespace.Engines.T2E.Economic.Capital.Binding;

public record BindingCommand(string Action, string EntityId, object Payload);

public sealed record CreateBindingCommand(string Id, string IdentityId, string WalletId)
    : BindingCommand("Create", Id, null!);

public sealed record RevokeBindingCommand(string BindingId)
    : BindingCommand("Revoke", BindingId, null!);
