using System.ComponentModel.DataAnnotations;
using diploma.Data.Common;
using diploma.Features.ContestApplications;
using diploma.Features.Problems;
using diploma.Features.Users;
using Sieve.Attributes;

namespace diploma.Features.Contests;

public class Contest : AuditableEntity
{
    [Sieve(CanFilter = true)]
    public Guid Id { get; set; }
    
    [MaxLength(150)]
    public string Name { get; set; } = null!;
    
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