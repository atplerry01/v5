using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public sealed class Viewer
{
    public string ViewerRef { get; }
    public Timestamp JoinedAt { get; }
    public Timestamp? LeftAt { get; private set; }

    private Viewer(string viewerRef, Timestamp joinedAt)
    {
        ViewerRef = viewerRef;
        JoinedAt = joinedAt;
    }

    public static Viewer Join(string viewerRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(viewerRef)) throw StreamSessionErrors.InvalidViewer();
        return new Viewer(viewerRef, at);
    }

    public void Leave(Timestamp at)
    {
        if (LeftAt.HasValue) throw StreamSessionErrors.ViewerAlreadyLeft();
        LeftAt = at;
    }

    public bool IsActive => !LeftAt.HasValue;
}
