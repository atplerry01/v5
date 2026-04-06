using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Constitutional.Chain;

public interface IChainDomainService
{
    Task<ChainBlockResult> CreateBlockAsync(DomainExecutionContext context, Guid blockId, string previousHash, object payload, string payloadType, string payloadId, string correlationId, DateTimeOffset timestamp, long sequenceNumber);
}

public sealed record ChainBlockResult(Guid Id, string CurrentHash, string PayloadHash, string PreviousHash);
