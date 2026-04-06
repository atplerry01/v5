namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed class SufficientBalanceSpec
{
    public bool IsSatisfiedBy(decimal balance, decimal amount) => balance >= amount;
}
