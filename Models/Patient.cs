using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Models;

[Table("Patient", Schema = "Healthcare")]
public class Patient
{
    [Key]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Navigation property
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
