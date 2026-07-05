using System.ComponentModel.DataAnnotations;
using contester.Features.Audit;
using contester.Features.Users;
using Sieve.Attributes;

namespace contester.Features.UserGroups;

public class UserGroup : AuditableEntity
{
    [Sieve(CanFilter =  true)]
    public Guid Id { get; set; }
    [MaxLength(255)] [Sieve(CanFilter = true)]
    public string Name { get; set; } = string.Empty;
    public List<User> MemberUsers { get; set; } = new();
    public List<UserGroup> MemberGroups { get; set; } = new();
    public List<UserGroup> ParentGroups { get; set; } = new();
    public bool IsSystemGroup { get; set; }
}
