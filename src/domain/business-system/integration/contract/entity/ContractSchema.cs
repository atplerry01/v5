namespace Whycespace.Domain.BusinessSystem.Integration.Contract;

public sealed class ContractSchema
{
    public ContractSchemaId SchemaId { get; }
    public string SchemaName { get; }
    public string SchemaDefinition { get; }

    public ContractSchema(ContractSchemaId schemaId, string schemaName, string schemaDefinition)
    {
        if (schemaId == default)
            throw new ArgumentException("SchemaId must not be empty.", nameof(schemaId));

        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("SchemaName must not be empty.", nameof(schemaName));

        if (string.IsNullOrWhiteSpace(schemaDefinition))
            throw new ArgumentException("SchemaDefinition must not be empty.", nameof(schemaDefinition));

        SchemaId = schemaId;
        SchemaName = schemaName;
        SchemaDefinition = schemaDefinition;
    }
}
