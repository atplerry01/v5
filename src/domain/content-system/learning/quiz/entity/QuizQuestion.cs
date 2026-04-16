namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed class QuizQuestion
{
    public Guid QuestionId { get; }
    public string Prompt { get; }
    public int Points { get; }

    private QuizQuestion(Guid questionId, string prompt, int points)
    {
        QuestionId = questionId;
        Prompt = prompt;
        Points = points;
    }

    public static QuizQuestion Create(Guid questionId, string prompt, int points)
    {
        if (questionId == Guid.Empty) throw QuizErrors.InvalidQuestion();
        if (string.IsNullOrWhiteSpace(prompt)) throw QuizErrors.InvalidQuestion();
        if (points <= 0) throw QuizErrors.InvalidQuestion();
        return new QuizQuestion(questionId, prompt.Trim(), points);
    }
}
