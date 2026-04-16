using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public sealed class MessageAttachment
{
    public Guid AttachmentId { get; }
    public string Uri { get; }
    public long SizeBytes { get; }

    private MessageAttachment(Guid attachmentId, string uri, long sizeBytes)
    {
        AttachmentId = attachmentId;
        Uri = uri;
        SizeBytes = sizeBytes;
    }

    public static MessageAttachment Create(Guid attachmentId, string uri, long sizeBytes)
    {
        if (attachmentId == Guid.Empty)
            throw MessagingErrors.InvalidAttachment();
        if (string.IsNullOrWhiteSpace(uri) || !System.Uri.TryCreate(uri, UriKind.Absolute, out _))
            throw MessagingErrors.InvalidAttachment();
        if (sizeBytes <= 0)
            throw MessagingErrors.InvalidAttachment();
        return new MessageAttachment(attachmentId, uri.Trim(), sizeBytes);
    }
}
