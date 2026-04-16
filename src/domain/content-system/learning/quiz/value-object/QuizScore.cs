using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed record QuizScore : ValueObject
{
    public int Correct { get; }
    public int Total { get; }

    private QuizScore(int correct, int total)
    {
        Correct = correct;
        Total = total;
    }

    public static QuizScore Create(int correct, int total)
    {
        if (total <= 0) throw QuizErrors.InvalidScore();
        if (correct < 0 || correct > total) throw QuizErrors.InvalidScore();
        return new QuizScore(correct, total);
    }

    public decimal Percentage => (decimal)Correct / Total * 100m;

    public override string ToString() => $"{Correct}/{Total}";
}
