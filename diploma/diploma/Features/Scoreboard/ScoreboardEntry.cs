namespace diploma.Features.Scoreboard;

public class ScoreboardProblemEntry
{
    public Guid ProblemId { get; set; }
    public string Name { get; set; } = null!;
    public int AttemptsCount { get; set; }
    public bool IsSolved { get; set; }
    public int MaxGrade { get; set; }
    public int Grade { get; set; }
    public DateTime? SolvedAt { get; set; }
    public Guid? SolvingAttemptId { get; set; }
}

public class ScoreboardEntry
{
    public Guid ContestId { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; } = null!;
    public string AdditionalInfo { get; set; } = null!;
    public int Fee { get; set; }
    public int FinalGrade { get; set; }
    public List<ScoreboardProblemEntry> Problems { get; set; } = null!;
}
