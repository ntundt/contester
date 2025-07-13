using System.ComponentModel.DataAnnotations;
using contester.Data.Common;
using contester.Features.Users;

namespace contester.Features.AttachedFiles;

public class AttachedFile : AuditableEntity
{
    public Guid Id { get; set; }
    [MaxLength(255)]
    public string OriginalName { get; set; } = null!;
    [MaxLength(255)]
    public string MimeType { get; set; } = null!;
    [MaxLength(255)]
    public string FilePath { get; set; } = null!;
    public Guid? AuthorId { get; set; }
    public User Author { get; set; } = null!;
}
