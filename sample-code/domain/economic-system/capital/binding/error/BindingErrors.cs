namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public static class BindingErrors
{
    public static DomainException AlreadyBound(Guid identityId, Guid walletId) =>
        new("BINDING_ALREADY_EXISTS", $"Identity {identityId} is already bound to wallet {walletId}.");

    public static DomainException AlreadyRevoked(Guid bindingId) =>
        new("BINDING_ALREADY_REVOKED", $"Binding {bindingId} has already been revoked.");

    public static DomainException UnauthorizedBinding(Guid identityId) =>
        new("BINDING_UNAUTHORIZED", $"Identity {identityId} is not authorized to create this binding.");
}
