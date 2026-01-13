using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Models;

[Table("Appointment", Schema = "Healthcare")]
public class Appointment
{
    [Key]
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "Patient is required")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Doctor is required")]
    [Display(Name = "Doctor")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Appointment date is required")]
    [Display(Name = "Appointment Date")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Appointment time is required")]
    [Display(Name = "Appointment Time")]
    [DataType(DataType.Time)]
    public TimeSpan AppointmentTime { get; set; }

    [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }

    [Required]
    [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    public string Status { get; set; } = "Scheduled";

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("PatientId")]
    public Patient Patient { get; set; } = null!;

    [ForeignKey("DoctorId")]
    public Doctor Doctor { get; set; } = null!;

    public ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
}
