using System.ComponentModel.DataAnnotations;
using contester.Features.Audit;
using contester.Features.ContestApplications;
using contester.Features.Problems;
using contester.Features.UserGroups;
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
    
    public UserGroup ParticipantsGroup { get; set; } = null!;
    public Guid ParticipantsGroupId { get; set; }
    public List<Problem> Problems { get; set; } = null!;
    public List<User> CommissionMembers { get; set; } = null!;
    public List<ContestApplication> ContestApplications { get; set; } = null!;
}
