using System.ComponentModel.DataAnnotations;
using contester.Data.Common;
using contester.Features.ContestApplications;
using contester.Features.Problems;
using contester.Features.Users;
using Sieve.Attributes;

namespace contester.Features.Contests;

public class Contest : AuditableEntity
{
    [Sieve(CanFilter = true)]
    public Guid Id { get; set; }
    
    [MaxLength(150)]
    public string Name { get; set; } = null!;
    
    [MaxLength(255)]
    public string DescriptionPath { get; set; } = null!;
    
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    
    [Sieve(CanSort = true)]
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public bool IsPublic { get; set; }
    
    /// <summary>
    /// Users who can participate in this contest. If the contest is public, this list is empty.
    /// </summary>
    public List<User> Participants { get; set; } = null!;
    public List<Problem> Problems { get; set; } = null!;
    public List<User> CommissionMembers { get; set; } = null!;
    public List<ContestApplication> ContestApplications { get; set; } = null!;
}