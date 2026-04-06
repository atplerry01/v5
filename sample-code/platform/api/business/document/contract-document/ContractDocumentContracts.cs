namespace Whycespace.Platform.Api.Business.Document.ContractDocument;

public sealed record ContractDocumentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ContractDocumentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
