namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed class CallbackDefinition
{
    public CallbackDefinitionId DefinitionId { get; }
    public string CallbackName { get; }
    public string ContractDescription { get; }

    public CallbackDefinition(CallbackDefinitionId definitionId, string callbackName, string contractDescription)
    {
        if (definitionId == default)
            throw new ArgumentException("DefinitionId must not be empty.", nameof(definitionId));

        if (string.IsNullOrWhiteSpace(callbackName))
            throw new ArgumentException("CallbackName must not be empty.", nameof(callbackName));

        if (string.IsNullOrWhiteSpace(contractDescription))
            throw new ArgumentException("ContractDescription must not be empty.", nameof(contractDescription));

        DefinitionId = definitionId;
        CallbackName = callbackName;
        ContractDescription = contractDescription;
    }
}
