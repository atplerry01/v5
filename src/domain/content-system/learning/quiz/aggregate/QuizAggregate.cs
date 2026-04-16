using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed class QuizAggregate : AggregateRoot
{
    private static readonly QuizSpecification Spec = new();
    private readonly Dictionary<Guid, QuizQuestion> _questions = new();

    public QuizId QuizId { get; private set; }
    public string CourseRef { get; private set; } = string.Empty;
    public QuizStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public IReadOnlyCollection<QuizQuestion> Questions => _questions.Values;

    private QuizAggregate() { }

    public static QuizAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        QuizId id, string courseRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(courseRef)) throw QuizErrors.InvalidCourseRef();
        var agg = new QuizAggregate();
        agg.RaiseDomainEvent(new QuizCreatedEvent(eventId, aggregateId, correlationId, causationId, id, courseRef, at));
        return agg;
    }

    public void AddQuestion(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, QuizQuestion question, Timestamp at)
    {
        Spec.EnsureDraftMutable(Status);
        if (_questions.ContainsKey(question.QuestionId)) throw QuizErrors.DuplicateQuestion(question.QuestionId);
        RaiseDomainEvent(new QuizQuestionAddedEvent(eventId, aggregateId, correlationId, causationId, QuizId, question.QuestionId, question.Prompt, question.Points, at));
    }

    public void Publish(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == QuizStatus.Published) throw QuizErrors.AlreadyPublished();
        if (Status == QuizStatus.Archived) throw QuizErrors.AlreadyArchived();
        if (_questions.Count == 0) throw QuizErrors.CannotPublishEmpty();
        RaiseDomainEvent(new QuizPublishedEvent(eventId, aggregateId, correlationId, causationId, QuizId, at));
    }

    public void Score(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string learnerRef, QuizScore score, Timestamp at)
    {
        if (Status != QuizStatus.Published) throw new DomainException("Only published quizzes may be scored.");
        if (string.IsNullOrWhiteSpace(learnerRef)) throw new DomainException("Learner ref must be non-empty.");
        RaiseDomainEvent(new QuizScoredEvent(eventId, aggregateId, correlationId, causationId, QuizId, learnerRef, score.Correct, score.Total, at));
    }

    public void Archive(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == QuizStatus.Archived) throw QuizErrors.AlreadyArchived();
        RaiseDomainEvent(new QuizArchivedEvent(eventId, aggregateId, correlationId, causationId, QuizId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case QuizCreatedEvent e:
                QuizId = e.QuizId;
                CourseRef = e.CourseRef;
                Status = QuizStatus.Draft;
                CreatedAt = e.CreatedAt;
                break;
            case QuizQuestionAddedEvent e:
                _questions[e.QuestionId] = QuizQuestion.Create(e.QuestionId, e.Prompt, e.Points);
                break;
            case QuizPublishedEvent: Status = QuizStatus.Published; break;
            case QuizArchivedEvent: Status = QuizStatus.Archived; break;
            case QuizScoredEvent: break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(CourseRef))
            throw QuizErrors.CourseMissing();
    }
}
