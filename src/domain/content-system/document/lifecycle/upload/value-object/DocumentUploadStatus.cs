namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public enum DocumentUploadStatus
{
    Requested,
    Accepted,
    Processing,
    Completed,
    Failed,
    Cancelled
}
