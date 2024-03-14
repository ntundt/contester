using diploma.Data.Common;
using diploma.Features.Users;

namespace diploma.Features.AttachedFiles;

public class AttachedFile : AuditableEntity
{
    public Guid Id { get; set; }
    public string OriginalName { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public Guid? AuthorId { get; set; }
    public User Author { get; set; } = null!;
}
