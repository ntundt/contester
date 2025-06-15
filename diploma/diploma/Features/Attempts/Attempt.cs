using System.ComponentModel.DataAnnotations;
using diploma.Data.Common;
using diploma.Features.Problems;
using diploma.Features.Users;
using Sieve.Attributes;

namespace diploma.Features.Attempts;

public enum AttemptStatus
{
    Pending,
    Error,
    WrongAnswer,
    WrongOutputFormat,
    TimeLimitExceeded,
    Accepted
}

public class Attempt : AuditableEntity
{
    public Guid Id { get; set; }
    [Sieve(CanFilter = true)]
    public Guid ProblemId { get; set; }
    public Problem Problem { get; set; } = null!;
    [Sieve(CanFilter = true)]
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    [MaxLength(255)]
    public string SolutionPath { get; set; } = null!;
    [MaxLength(255)]
    public string Dbms { get; set; } = null!;
    [Sieve(CanFilter = true)]
    public AttemptStatus Status { get; set; }
    [MaxLength(128)]
    public string? ErrorMessage { get; set; }
    
    [Sieve(CanSort = true)]
    public override DateTime CreatedAt { get; set; }

    public int? Originality { get; set; }
    public Attempt? OriginalAttempt { get; set; } = null!;
    public Guid? OriginalAttemptId { get; set; } = null!;
}