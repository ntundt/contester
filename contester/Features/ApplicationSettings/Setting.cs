using System.ComponentModel.DataAnnotations;
using contester.Features.Audit;

namespace contester.Features.ApplicationSettings;

public class Setting : AuditableEntity
{
    [Key]
    public required string Key { get; set; }
    public required string Value { get; set; }
}
