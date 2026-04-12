namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed class OwnershipValidationService
{
    public bool ValidateUniqueOwnership(
        BindingAggregate binding,
        IReadOnlyList<BindingAggregate> existingBindings)
    {
        return !existingBindings.Any(b =>
            b.AccountId == binding.AccountId
            && b.Status == BindingStatus.Active
            && !b.BindingId.Equals(binding.BindingId));
    }
}
