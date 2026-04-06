namespace Whycespace.Shared.Contracts.Domain;

/// <summary>
/// Maps domain events to flattened ACL contract DTOs and back.
/// Implementations live outside the domain — this is the pure translation boundary.
/// </summary>
public interface IEventContractMapper<TDomainEvent, TContractDTO>
{
    TContractDTO ToContract(TDomainEvent domainEvent);
    TDomainEvent ToDomain(TContractDTO contract);
    int ContractVersion { get; }
    bool CanMap(string schemaId, int version);
}