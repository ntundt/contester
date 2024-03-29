using diploma.Data.Common;
using diploma.Features.Contests;
using diploma.Features.Users;

namespace diploma.Features.Scoreboard;

public class ScoreboardApproval : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ApprovingUserId { get; set; }
    public User ApprovingUser { get; set; } = null!;
    public Guid ContestId { get; set; }
    public Contest Contest { get; set; } = null!;
}