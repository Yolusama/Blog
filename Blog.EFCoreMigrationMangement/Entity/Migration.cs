using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.EFCoreMigration.Entity;

[Table(nameof(Migration))]
public class Migration
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime? CreateTime { get; set; }
    public string FileName { get; set; }
    [Column(TypeName = "integer")]
    public bool HasApplied { get; set; }
    public DateTime? ApplyTime { get; set; }
}