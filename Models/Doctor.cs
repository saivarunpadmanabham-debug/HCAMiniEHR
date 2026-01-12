using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Models;

[Table("Doctor", Schema = "Healthcare")]
public class Doctor
{
    [Key]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required")]
    [MaxLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
    public string Specialization { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "License number cannot exceed 50 characters")]
    [Display(Name = "License Number")]
    public string? LicenseNumber { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [NotMapped]
    [Display(Name = "Full Name")]
    public string FullName => $"Dr. {FirstName} {LastName}";

    [NotMapped]
    public string FullNameWithSpecialization => $"Dr. {FirstName} {LastName} ({Specialization})";
}
