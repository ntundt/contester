using contester.Features.Audit;
using contester.Features.Users;

namespace contester.Features.UserGroups;

public class UserGroup : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<User> MemberUsers { get; set; } = new();
    public List<UserGroup> MemberGroups { get; set; } = new();
}
