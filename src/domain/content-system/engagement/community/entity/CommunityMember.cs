using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Community;

public sealed class CommunityMember
{
    public string MemberRef { get; }
    public CommunityRole Role { get; private set; }
    public Timestamp JoinedAt { get; }
    public Timestamp? LeftAt { get; private set; }

    private CommunityMember(string memberRef, CommunityRole role, Timestamp joinedAt)
    {
        MemberRef = memberRef;
        Role = role;
        JoinedAt = joinedAt;
    }

    public static CommunityMember Join(string memberRef, CommunityRole role, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(memberRef)) throw CommunityErrors.InvalidMember();
        return new CommunityMember(memberRef, role, at);
    }

    public void AssignRole(CommunityRole role) => Role = role;

    public void Leave(Timestamp at)
    {
        if (LeftAt.HasValue) throw CommunityErrors.MemberAlreadyLeft();
        LeftAt = at;
    }

    public bool IsActive => !LeftAt.HasValue;
}
