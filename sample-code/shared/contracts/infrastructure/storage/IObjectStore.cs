namespace Whycespace.Shared.Contracts.Infrastructure.Storage;

/// <summary>
/// Infrastructure contract for S3-compatible object storage.
/// Implementations live in infrastructure/adapters — never in domain or engines.
/// </summary>
public interface IObjectStore
{
    Task<ObjectUploadResult> UploadAsync(
        Stream content,
        string bucket,
        string objectKey,
        string? contentType = null,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken = default);
}

public sealed record ObjectUploadResult(
    string ObjectUrl,
    string Checksum,
    string Bucket,
    string ObjectKey,
    long ContentLength);
