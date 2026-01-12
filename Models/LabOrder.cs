using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Models;

[Table("LabOrder", Schema = "Healthcare")]
public class LabOrder
{
    [Key]
    public int LabOrderId { get; set; }

    [Required]
    public int AppointmentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TestName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.Now;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(1000)]
    public string? Results { get; set; }

    public DateTime? CompletedDate { get; set; }

    // Navigation property
    [ForeignKey("AppointmentId")]
    public Appointment Appointment { get; set; } = null!;
}
