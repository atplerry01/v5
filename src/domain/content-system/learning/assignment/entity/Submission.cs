using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed class Submission
{
    public Guid SubmissionId { get; }
    public string LearnerRef { get; }
    public string ContentRef { get; }
    public Timestamp SubmittedAt { get; }
    public AssignmentGrade? Grade { get; private set; }
    public Timestamp? GradedAt { get; private set; }

    private Submission(Guid submissionId, string learnerRef, string contentRef, Timestamp submittedAt)
    {
        SubmissionId = submissionId;
        LearnerRef = learnerRef;
        ContentRef = contentRef;
        SubmittedAt = submittedAt;
    }

    public static Submission Receive(Guid submissionId, string learnerRef, string contentRef, Timestamp at)
    {
        if (submissionId == Guid.Empty) throw AssignmentErrors.InvalidSubmission();
        if (string.IsNullOrWhiteSpace(learnerRef)) throw AssignmentErrors.InvalidSubmission();
        if (string.IsNullOrWhiteSpace(contentRef)) throw AssignmentErrors.InvalidSubmission();
        return new Submission(submissionId, learnerRef, contentRef, at);
    }

    public void AssignGrade(AssignmentGrade grade, Timestamp at)
    {
        if (Grade is not null) throw AssignmentErrors.AlreadyGraded();
        Grade = grade;
        GradedAt = at;
    }

    public bool IsGraded => Grade is not null;
}
