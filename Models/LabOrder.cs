using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Models;

[Table("LabOrder", Schema = "Healthcare")]
public class LabOrder
{
    [Key]
    public int LabOrderId { get; set; }

    [Display(Name = "Appointment")]
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "Test name is required")]
    [MaxLength(200, ErrorMessage = "Test name cannot exceed 200 characters")]
    [Display(Name = "Test Name")]
    public string TestName { get; set; } = string.Empty;

    [Display(Name = "Order Date")]
    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    public string Status { get; set; } = "Pending";

    [MaxLength(1000, ErrorMessage = "Results cannot exceed 1000 characters")]
    public string? Results { get; set; }

    [Display(Name = "Completed Date")]
    [DataType(DataType.Date)]
    public DateTime? CompletedDate { get; set; }

    // Navigation property
    [ForeignKey("AppointmentId")]
    public Appointment Appointment { get; set; } = null!;
}
