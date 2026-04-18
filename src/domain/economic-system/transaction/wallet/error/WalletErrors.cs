using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public static class WalletErrors
{
    public static DomainException WalletNotActive() =>
        new("Wallet must be Active to perform this operation.");

    public static DomainException InvalidOwnerId() =>
        new("Owner ID cannot be empty.");

    public static DomainException InvalidAccountId() =>
        new("Account ID cannot be empty.");

    public static DomainException NoAccountMapped() =>
        new("Wallet must be mapped to an account before requesting transactions.");

    public static DomainException InvalidAmount() =>
        new("Transaction amount must be greater than zero.");

    public static DomainException InvalidDestination() =>
        new("Destination account must be specified.");

    public static DomainException InvalidRequestId() =>
        new("RequestId (idempotency key) cannot be empty.");

    public static DomainInvariantViolationException WalletMustHaveAccount() =>
        new("Invariant violated: active wallet must be mapped to an account.");
}
