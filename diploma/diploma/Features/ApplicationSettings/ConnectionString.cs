using System.ComponentModel.DataAnnotations;

namespace diploma.Features.ApplicationSettings;

public class ConnectionString
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Dbms { get; set; } = null!;
    [MaxLength(255)]
    public string Text { get; set; } = null!;
}
