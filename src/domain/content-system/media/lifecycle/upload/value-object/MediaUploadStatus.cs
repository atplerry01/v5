namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public enum MediaUploadStatus
{
    Requested,
    Accepted,
    Processing,
    Completed,
    Failed,
    Cancelled
}
