namespace Whycespace.Shared.Contracts.Domain.Constitutional.Chain;

/// <summary>
/// Chain validation contracts — engines use these instead of domain ChainErrors/ChainIntegritySpec.
/// </summary>
public interface IChainHashService
{
    string SerializePayload(object payload);
}

public static class ChainValidationErrors
{
    public static InvalidOperationException ChainHeadMismatch(string expectedHash, string actualHash)
        => new($"Chain head mismatch — caller expected '{expectedHash}' but actual head is '{actualHash}'. Possible concurrent fork attempt.");
}
