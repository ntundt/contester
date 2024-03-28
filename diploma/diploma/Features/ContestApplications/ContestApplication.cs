using diploma.Data.Common;
using diploma.Features.Contests;
using diploma.Features.Users;

namespace diploma.Features.ContestApplications;

/// <summary>
/// An application to participate in a contest that's not public.
/// </summary>
public class ContestApplication : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ContestId { get; set; }
    public Contest Contest { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsApproved { get; set; }
}
