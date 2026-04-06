namespace Whycespace.Domain.OperationalSystem.Deployment.Sandbox;

public sealed class SandboxService
{
    public bool CanRecordTransaction(EconomicSandboxAggregate sandbox, decimal amount) =>
        sandbox.Status == SandboxStatus.Active
        && sandbox.TransactionCount < sandbox.TransactionLimit
        && sandbox.CurrentCapital + amount <= sandbox.CapitalCap;

    public decimal RemainingCapital(EconomicSandboxAggregate sandbox) =>
        sandbox.CapitalCap - sandbox.CurrentCapital;
}
