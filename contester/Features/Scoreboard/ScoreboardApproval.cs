using contester.Data.Common;
using contester.Features.Contests;
using contester.Features.Users;

namespace contester.Features.Scoreboard;

public class ScoreboardApproval : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ApprovingUserId { get; set; }
    public User ApprovingUser { get; set; } = null!;
    public Guid ContestId { get; set; }
    public Contest Contest { get; set; } = null!;
}