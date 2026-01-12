using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Models;

[Table("AuditLog", Schema = "Healthcare")]
public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

    [Required]
    public int RecordId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ChangedDate { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string ChangedBy { get; set; } = "SYSTEM";
}
