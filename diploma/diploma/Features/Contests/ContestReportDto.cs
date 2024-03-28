namespace diploma.Features.Contests;

public class ContestReportDto
{
    public Guid ContestId { get; set; }
    public string ContestName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public bool IsPublic { get; set; }
    public List<ContestReportUserDto> Participants { get; set; } = null!;
}

public class ContestReportUserDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string Email { get; set; } = null!;
    public string AdditionalInfo { get; set; } = null!;
    public int Score { get; set; }
    public int AttemptsCount { get; set; }
    public DateTime LastLoginUtc { get; set; }
    public DateTime SignedUpUtc { get; set; }
}
