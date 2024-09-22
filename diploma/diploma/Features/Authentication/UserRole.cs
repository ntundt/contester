using System.ComponentModel.DataAnnotations;

namespace diploma.Features.Authentication;

public class UserRole
{
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    public List<Permission> Permissions { get; set; } = null!;
}
