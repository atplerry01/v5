namespace Whycespace.Engines.T2E.Business.Document.ContractDocument;

public record ContractDocumentCommand(string Action, string EntityId, object Payload);
public sealed record CreateContractDocumentCommand(string Id) : ContractDocumentCommand("Create", Id, null!);
