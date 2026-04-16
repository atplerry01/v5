using Whycespace.Shared.Contracts.Content.Learning.Course;
using Whycespace.Shared.Contracts.Events.Content.Learning.Course;

namespace Whycespace.Projections.Content.Learning.Course.Reducer;

/// <summary>
/// Pure state reducer for the Course read model. No I/O, no side effects.
/// </summary>
public static class CourseProjectionReducer
{
    public static CourseReadModel Apply(CourseReadModel state, CourseDraftedEventSchema e)
        => state with
        {
            OwnerRef = e.OwnerRef,
            Title = e.Title,
            Status = "draft",
            DraftedAt = e.DraftedAt
        };

    public static CourseReadModel Apply(CourseReadModel state, CourseModuleAttachedEventSchema e)
    {
        var next = new Dictionary<string, int>(state.Outline, StringComparer.Ordinal)
        {
            [e.ModuleRef] = e.Order
        };
        return state with { Outline = next, LastTransitionedAt = e.AttachedAt };
    }

    public static CourseReadModel Apply(CourseReadModel state, CourseModuleDetachedEventSchema e)
    {
        var next = new Dictionary<string, int>(state.Outline, StringComparer.Ordinal);
        next.Remove(e.ModuleRef);
        return state with { Outline = next, LastTransitionedAt = e.DetachedAt };
    }

    public static CourseReadModel Apply(CourseReadModel state, CoursePublishedEventSchema e)
        => state with { Status = "published", LastTransitionedAt = e.PublishedAt };

    public static CourseReadModel Apply(CourseReadModel state, CourseArchivedEventSchema e)
        => state with { Status = "archived", LastTransitionedAt = e.ArchivedAt };
}
