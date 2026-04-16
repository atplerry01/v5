using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed class AssignmentAggregate : AggregateRoot
{
    private static readonly AssignmentSpecification Spec = new();
    private readonly Dictionary<Guid, Submission> _submissions = new();

    public AssignmentId AssignmentId { get; private set; }
    public string CourseRef { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public decimal MaxGrade { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public IReadOnlyCollection<Submission> Submissions => _submissions.Values;

    private AssignmentAggregate() { }

    public static AssignmentAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        AssignmentId id, string courseRef, string title, decimal maxGrade, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(courseRef)) throw AssignmentErrors.InvalidCourseRef();
        if (string.IsNullOrWhiteSpace(title)) throw AssignmentErrors.InvalidTitle();
        if (maxGrade <= 0m) throw AssignmentErrors.InvalidGrade();
        var agg = new AssignmentAggregate();
        agg.RaiseDomainEvent(new AssignmentCreatedEvent(eventId, aggregateId, correlationId, causationId, id, courseRef, title.Trim(), maxGrade, at));
        return agg;
    }

    public void Publish(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsurePublishable(Status);
        RaiseDomainEvent(new AssignmentPublishedEvent(eventId, aggregateId, correlationId, causationId, AssignmentId, at));
    }

    public void ReceiveSubmission(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Submission submission, Timestamp at)
    {
        Spec.EnsurePublished(Status);
        if (_submissions.ContainsKey(submission.SubmissionId))
            throw AssignmentErrors.InvalidSubmission();
        RaiseDomainEvent(new AssignmentSubmissionReceivedEvent(
            eventId, aggregateId, correlationId, causationId, AssignmentId, submission.SubmissionId, submission.LearnerRef, submission.ContentRef, at));
    }

    public void GradeSubmission(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Guid submissionId, AssignmentGrade grade, Timestamp at)
    {
        if (!_submissions.TryGetValue(submissionId, out var submission))
            throw AssignmentErrors.UnknownSubmission(submissionId);
        if (submission.IsGraded) throw AssignmentErrors.AlreadyGraded();
        if (grade.Max != MaxGrade) throw AssignmentErrors.InvalidGrade();
        RaiseDomainEvent(new AssignmentSubmissionGradedEvent(
            eventId, aggregateId, correlationId, causationId, AssignmentId, submissionId, grade.Value, grade.Max, at));
    }

    public void Close(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == AssignmentStatus.Closed) throw AssignmentErrors.AlreadyClosed();
        RaiseDomainEvent(new AssignmentClosedEvent(eventId, aggregateId, correlationId, causationId, AssignmentId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AssignmentCreatedEvent e:
                AssignmentId = e.AssignmentId;
                CourseRef = e.CourseRef;
                Title = e.Title;
                MaxGrade = e.MaxGrade;
                Status = AssignmentStatus.Draft;
                CreatedAt = e.CreatedAt;
                break;
            case AssignmentPublishedEvent: Status = AssignmentStatus.Published; break;
            case AssignmentSubmissionReceivedEvent e:
                _submissions[e.SubmissionId] = Submission.Receive(e.SubmissionId, e.LearnerRef, e.ContentRef, e.ReceivedAt);
                break;
            case AssignmentSubmissionGradedEvent e:
                if (_submissions.TryGetValue(e.SubmissionId, out var s))
                    s.AssignGrade(AssignmentGrade.Create(e.Grade, e.MaxGrade), e.GradedAt);
                break;
            case AssignmentClosedEvent: Status = AssignmentStatus.Closed; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(CourseRef))
            throw AssignmentErrors.CourseMissing();
    }
}
