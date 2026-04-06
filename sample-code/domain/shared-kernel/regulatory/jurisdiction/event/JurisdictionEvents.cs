namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed record JurisdictionCreatedEvent(
    JurisdictionId JurisdictionId,
    JurisdictionCode Code,
    string Name,
    LegalSystem LegalSystem,
    RegulatoryFramework RegulatoryFramework) : DomainEvent;

public sealed record DataResidencyRuleAddedEvent(
    JurisdictionId JurisdictionId,
    string RuleCode,
    bool RequiresLocalStorage) : DomainEvent;

public sealed record JurisdictionRegulatoryFrameworkUpdatedEvent(
    JurisdictionId JurisdictionId,
    RegulatoryFramework Framework) : DomainEvent;
