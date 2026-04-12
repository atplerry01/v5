namespace Whycespace.Domain.BusinessSystem.Integration.Contract;

public sealed record ContractCreatedEvent(ContractId ContractId, ContractSchemaId SchemaId, string SchemaName);
