using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public static class ContractErrors
{
    public static DomainException InsufficientParties() =>
        new("Contract must have at least 2 parties.");

    public static DomainException InvalidTerm() =>
        new("Contract end date must be after start date.");

    public static DomainException InvalidSharePercentage() =>
        new("Each share percentage must be between 0 and 100.");

    public static DomainException ContractNotDraft() =>
        new("Contract must be in Draft status to activate.");

    public static DomainException ContractNotActive() =>
        new("Contract must be Active to perform this operation.");

    public static DomainException ContractAlreadyTerminated() =>
        new("Contract has already been terminated.");

    public static DomainException ContractAlreadyActive() =>
        new("Contract is already active.");

    public static DomainException InvalidPartyId() =>
        new("Party ID cannot be empty.");

    public static DomainInvariantViolationException SharesMustTotal100(decimal actualTotal) =>
        new($"Invariant violated: revenue shares must total 100%, got {actualTotal:F2}%.");

    public static DomainInvariantViolationException InsufficientPartiesInvariant(int count) =>
        new($"Invariant violated: contract must have at least 2 parties, found {count}.");
}
