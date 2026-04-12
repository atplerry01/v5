namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed record ContractDocumentSectionAddedEvent(ContractDocumentId ContractDocumentId, Guid SectionId, string Title);
