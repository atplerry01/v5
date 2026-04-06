namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed class JurisdictionAggregate : AggregateRoot
{
    public JurisdictionCode Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public LegalSystem LegalSystem { get; private set; } = null!;
    public Currency Currency { get; private set; } = null!;
    public RegulatoryFramework RegulatoryFramework { get; private set; } = null!;
    public TaxAuthority TaxAuthority { get; private set; } = null!;

    private readonly List<DataResidencyRule> _dataResidencyRules = [];
    public IReadOnlyList<DataResidencyRule> DataResidencyRules => _dataResidencyRules.AsReadOnly();

    private JurisdictionAggregate() { }

    public static JurisdictionAggregate Create(
        Guid id,
        JurisdictionCode code,
        string name,
        LegalSystem legalSystem,
        Currency currency,
        RegulatoryFramework regulatoryFramework,
        TaxAuthority taxAuthority)
    {
        Guard.AgainstNull(code, nameof(code));
        Guard.AgainstEmpty(name, nameof(name));
        Guard.AgainstNull(legalSystem, nameof(legalSystem));
        Guard.AgainstNull(currency, nameof(currency));
        Guard.AgainstNull(regulatoryFramework, nameof(regulatoryFramework));
        Guard.AgainstNull(taxAuthority, nameof(taxAuthority));

        var aggregate = new JurisdictionAggregate
        {
            Id = id,
            Code = code,
            Name = name,
            LegalSystem = legalSystem,
            Currency = currency,
            RegulatoryFramework = regulatoryFramework,
            TaxAuthority = taxAuthority
        };

        aggregate.RaiseDomainEvent(new JurisdictionCreatedEvent(
            JurisdictionId: new JurisdictionId(id),
            Code: code,
            Name: name,
            LegalSystem: legalSystem,
            RegulatoryFramework: regulatoryFramework));

        return aggregate;
    }

    public void AddDataResidencyRule(DataResidencyRule rule)
    {
        Guard.AgainstNull(rule, nameof(rule));

        if (_dataResidencyRules.Any(r => r.RuleCode == rule.RuleCode))
            throw new DomainException(JurisdictionErrors.DuplicateResidencyRule,
                $"Data residency rule '{rule.RuleCode}' already exists for jurisdiction '{Code.Value}'.");

        _dataResidencyRules.Add(rule);

        RaiseDomainEvent(new DataResidencyRuleAddedEvent(
            JurisdictionId: new JurisdictionId(Id),
            RuleCode: rule.RuleCode,
            RequiresLocalStorage: rule.RequiresLocalStorage));
    }

    public void UpdateRegulatoryFramework(RegulatoryFramework framework)
    {
        Guard.AgainstNull(framework, nameof(framework));
        RegulatoryFramework = framework;

        RaiseDomainEvent(new JurisdictionRegulatoryFrameworkUpdatedEvent(
            JurisdictionId: new JurisdictionId(Id),
            Framework: framework));
    }

    public bool RequiresLocalStorage()
    {
        return _dataResidencyRules.Any(r => r.RequiresLocalStorage);
    }
}
