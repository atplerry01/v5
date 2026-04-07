namespace Whyce.Engines.T0U.Determinism.Time;

/// <summary>
/// HSID v2.1 time-bucket provider. Returns the bucket segment (TTT, 3 chars)
/// for a given deterministic seed. Implementations MUST NOT read the system
/// clock — buckets are derived from the seed itself so replays are stable.
/// </summary>
public interface ITimeBucketProvider
{
    string GetBucket(string seed);
}
