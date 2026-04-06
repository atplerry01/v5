using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Economic.Transaction;

public interface ITransactionDomainService
{
    Task<DomainOperationResult> InitiateAsync(DomainExecutionContext context, string transactionId, string sourceWalletId, string destinationWalletId, decimal amount, string currencyCode);
    Task<DomainOperationResult> ApproveAsync(DomainExecutionContext context, string transactionId);
    Task<DomainOperationResult> CompleteAsync(DomainExecutionContext context, string transactionId);
    Task<DomainOperationResult> RejectAsync(DomainExecutionContext context, string transactionId, string reason);
    Task<DomainOperationResult> SettleAsync(DomainExecutionContext context, string transactionId);
}

public interface IWalletDomainService
{
    Task<DomainOperationResult> CreateAsync(DomainExecutionContext context, string walletId, string identityId, string currency);
    Task<DomainOperationResult> FreezeAsync(DomainExecutionContext context, string walletId, string reason);
    Task<DomainOperationResult> UnfreezeAsync(DomainExecutionContext context, string walletId);
}

public interface ILimitDomainService
{
    Task<DomainOperationResult> CreateAndEvaluateAsync(DomainExecutionContext context, string limitId, string identityId, decimal maxTransactionAmount, decimal dailyLimit, decimal monthlyLimit, decimal transactionAmount, decimal dailyTotal, decimal monthlyTotal);
}

public sealed class LimitEvaluationData
{
    public required string LimitId { get; init; }
    public required string IdentityId { get; init; }
    public required bool HasViolations { get; init; }
    public required List<LimitViolationData> Violations { get; init; }
}

public sealed class LimitViolationData
{
    public required string LimitType { get; init; }
}
