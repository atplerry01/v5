using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhyceChain.Write;

public sealed record WriteChainBlockCommand(
    Guid BlockId,
    object Payload,
    string PayloadType,
    string PayloadId,
    string CorrelationId,
    string ExpectedPreviousHash);

public sealed record WriteChainBlockResult(
    string BlockId,
    string CurrentHash,
    string PayloadHash,
    string PreviousHash,
    DateTimeOffset Timestamp);

public sealed class ChainWriteEngine
{
    private readonly IChainStore _chainStore;
    private readonly IClock _clock;
    private readonly IChainDomainService _chainDomainService;
    private readonly IChainHashService _hashService;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public ChainWriteEngine(
        IChainStore chainStore,
        IClock clock,
        IChainDomainService chainDomainService,
        IChainHashService hashService)
    {
        _chainStore = chainStore ?? throw new ArgumentNullException(nameof(chainStore));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _chainDomainService = chainDomainService ?? throw new ArgumentNullException(nameof(chainDomainService));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
    }

    public async Task<WriteChainBlockResult> ExecuteAsync(
        WriteChainBlockCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            // 1. Fetch last block hash
            var lastBlock = await _chainStore.GetLatestBlockAsync(cancellationToken);
            var previousHash = lastBlock?.Hash
                ?? Whycespace.Shared.Contracts.Infrastructure.Storage.ChainBlock.GenesisHash;

            // 1b. Enforce previous hash continuity — reject stale callers (fork prevention)
            if (command.ExpectedPreviousHash != previousHash)
                throw ChainValidationErrors.ChainHeadMismatch(command.ExpectedPreviousHash, previousHash);

            // 2. Compute new block via domain service
            var timestamp = _clock.UtcNowOffset;
            var sequenceNumber = (lastBlock?.SequenceNumber ?? 0) + 1;

            var execCtx = new DomainExecutionContext
            {
                CorrelationId = command.CorrelationId ?? DeterministicIdHelper.FromSeed($"{command.BlockId}:{command.PayloadType}:{command.PayloadId}:CreateBlock").ToString("N"),
                ActorId = "system",
                Action = "CreateBlock",
                Domain = "constitutional.chain",
                Timestamp = timestamp
            };

            var domainBlock = await _chainDomainService.CreateBlockAsync(
                execCtx,
                blockId: command.BlockId,
                previousHash: previousHash,
                payload: command.Payload,
                payloadType: command.PayloadType,
                payloadId: command.PayloadId,
                correlationId: command.CorrelationId ?? execCtx.CorrelationId,
                timestamp: timestamp,
                sequenceNumber: sequenceNumber);

            // 3. Append to store
            var serializedPayload = _hashService.SerializePayload(command.Payload);

            var storeBlock = new Whycespace.Shared.Contracts.Infrastructure.Storage.ChainBlock
            {
                BlockId = domainBlock.Id.ToString(),
                SequenceNumber = sequenceNumber,
                PreviousHash = domainBlock.PreviousHash,
                Hash = domainBlock.CurrentHash,
                PayloadHash = domainBlock.PayloadHash,
                Payload = serializedPayload,
                Timestamp = timestamp
            };

            await _chainStore.AppendBlockAsync(storeBlock, cancellationToken);

            // 4. ChainBlockCreatedEvent is already raised on the domain aggregate
            return new WriteChainBlockResult(
                BlockId: domainBlock.Id.ToString(),
                CurrentHash: domainBlock.CurrentHash,
                PayloadHash: domainBlock.PayloadHash,
                PreviousHash: domainBlock.PreviousHash,
                Timestamp: timestamp);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
