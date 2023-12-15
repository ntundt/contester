namespace diploma.Features.Scoreboard;

public class ScoreboardProblemEntryDto
{
    public Guid ProblemId { get; set; }
    public string Name { get; set; } = null!;
    public int AttemptsCount { get; set; }
    public bool IsSolved { get; set; }
    public DateTime SolvedAt { get; set; }
}

public class ScoreboardEntryDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Patronymic { get; set; } = null!;
    public string AdditionalInfo { get; set; } = null!;
    public int Fee { get; set; }
    public List<ScoreboardProblemEntryDto> Problems { get; set; } = null!;
}