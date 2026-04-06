namespace Whycespace.Domain.DecisionSystem.Compliance.Jurisdiction;

using Whycespace.Domain.SharedKernel;

public sealed class ComplianceJurisdiction : Entity
{
    public string Name { get; private set; } = default!;
    public string IsoCode { get; private set; } = default!;
    public string Region { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private readonly List<JurisdictionBinding> _bindings = [];
    public IReadOnlyList<JurisdictionBinding> Bindings => _bindings.AsReadOnly();

    private ComplianceJurisdiction() { }

    public static ComplianceJurisdiction Create(Guid jurisdictionId, string name, string isoCode, string region)
    {
        return new ComplianceJurisdiction
        {
            Id = jurisdictionId,
            Name = name,
            IsoCode = isoCode,
            Region = region,
            IsActive = true
        };
    }

    public void AddBinding(JurisdictionBinding binding)
    {
        if (_bindings.Any(b => b.RegulationId == binding.RegulationId))
            return;

        _bindings.Add(binding);
    }

    public void RemoveBinding(Guid regulationId)
    {
        var binding = _bindings.FirstOrDefault(b => b.RegulationId == regulationId);
        if (binding is not null)
            _bindings.Remove(binding);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
