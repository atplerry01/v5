using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Course;

public sealed class CourseOutlineItem
{
    public string ModuleRef { get; }
    public int Order { get; private set; }
    public Timestamp AddedAt { get; }

    private CourseOutlineItem(string moduleRef, int order, Timestamp addedAt)
    {
        ModuleRef = moduleRef;
        Order = order;
        AddedAt = addedAt;
    }

    public static CourseOutlineItem Attach(string moduleRef, int order, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(moduleRef)) throw CourseErrors.InvalidModuleRef();
        if (order < 0) throw CourseErrors.InvalidOrder();
        return new CourseOutlineItem(moduleRef, order, at);
    }

    public void Reorder(int newOrder)
    {
        if (newOrder < 0) throw CourseErrors.InvalidOrder();
        Order = newOrder;
    }
}
