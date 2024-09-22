using System.ComponentModel.DataAnnotations;

namespace diploma.Features.Authentication;

public class Permission
{
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    public List<UserRole> UserRoles { get; set; } = null!;
}
