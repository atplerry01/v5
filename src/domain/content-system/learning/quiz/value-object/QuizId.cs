namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public readonly record struct QuizId(Guid Value)
{
    public static QuizId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
