using contester.Data.Common;
using contester.Features.Contests;
using contester.Features.Users;

namespace contester.Features.ContestApplications;

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
